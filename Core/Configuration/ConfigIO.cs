using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;
using TerrariaOverhaul.Core.Debugging;
using TerrariaOverhaul.Core.Localization;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Core.Configuration;

//TODO: Add 'required' to all fields in .NET 8.
public struct ConfigFormat
{
	public delegate ConfigIO.Result ReadConfigDelegate(Stream stream, out ConfigExport configExport);
	public delegate ConfigIO.Result WriteConfigDelegate(Stream stream, in ConfigExport configExport);

	public ReadConfigDelegate ReadConfig;
	public WriteConfigDelegate WriteConfig;
	public string Extension;
}

public ref struct ConfigExport
{
	public Version? ModVersion = null;
	public Dictionary<string, object> ValuesByEntry = new();

	public ConfigExport() { }
}

public sealed class ConfigIO : ModSystem
{
	public ref struct LoadingEntryContext
	{
		//TODO:
		//public ref ConfigExport FileInfo;
		public ConfigExport FileInfo; 
		public IConfigEntry Entry;
		public object? Value;
	}

	public delegate bool LoadingEntryPredicate(in LoadingEntryContext context);

	private static readonly List<LoadingEntryPredicate> configEntryResetPredicates = new();

	public static string Directory { get; } = OverhaulMod.PersonalDirectory;
	public static string FileName { get; } = "Config";

	public static string FilePathWithoutExtension => Path.Combine(OverhaulMod.PersonalDirectory, FileName);
	public static ReadOnlySpan<ConfigFormat> Formats => formats;

	private static readonly ConfigFormat[] formats = {
		// Main:
		TomlConfig.Format,
		// Fallbacks:
		JsonConfig.Format,
	};
	private static FileSystemWatcher? configWatcher;
	private static DateTime lastConfigWatcherWriteTime;
	private static volatile int ignoreConfigWatcherCounter;
	private static bool isInitialized;
	private static bool isUnloading;

	// Sure would be neat to have tagged unions and not be a caveman.
	private const int ResultOffset = 3;
	public enum Result
	{
		SuccessFlag = 1,    // 00000001
		WarningFlag = 2,    // 00000010
		ErrorFlag = 4,      // 00000100

		// Successes
		Success = (1 << ResultOffset) | SuccessFlag,
		// Warnings
		HadErrors = (1 << ResultOffset) | WarningFlag,
		ModVersionMissing = (2 << ResultOffset) | WarningFlag,
		// Errors
		FileMissing = (1 << ResultOffset) | ErrorFlag,
		FileBroken = (2 << ResultOffset) | ErrorFlag,
		FileInaccessible = (4 << ResultOffset) | ErrorFlag,
	}

	public override void Unload() => Deinitialize();

	private static void EnsureInitialized()
	{
		if (isInitialized || isUnloading) {
			return;
		}

		try {
			configWatcher = new FileSystemWatcher(Directory) {
				EnableRaisingEvents = true,
				NotifyFilter = NotifyFilters.LastWrite,
			};

			configWatcher.Changed += OnConfigDirectoryFileUpdateChanged;
		}
		catch {
			DebugSystem.Logger.Error($"Could not start a {nameof(FileSystemWatcher)} instance for the configuration file. Automatic config file reloading will be disabled.");
		}

		SetupMaintenance();

		isInitialized = true;
	}

	private static void Deinitialize()
	{
		isUnloading = true;

		if (isInitialized) {
			isInitialized = false;

			if (configWatcher != null) {
				configWatcher.Changed -= OnConfigDirectoryFileUpdateChanged;
				configWatcher.Dispose();
			}

			configEntryResetPredicates.Clear();
		}
	}

	// Legacy config handling
	private static void SetupMaintenance()
	{
		// Reset ranged options saved prior to 'v5.0-beta-12b', as their default values were bugged.
		RegisterEntryResetPredicate((in LoadingEntryContext context) => context is {
			Entry: RangeConfigEntry<float>,
			FileInfo.ModVersion: {
				Major: <= 5,
				Minor: <= 0,
				Build: <= 0,
				Revision: <= 24,
			}
		});
	}

	public static void RegisterEntryResetPredicate(LoadingEntryPredicate predicate)
	{
		configEntryResetPredicates.Add(predicate);
	}

	public static bool ShouldResetEntry(in LoadingEntryContext context)
	{
		foreach (var predicate in configEntryResetPredicates) {
			if (predicate(in context)) {
				return true;
			}
		}

		return false;
	}

	/// <summary> Imports values into config entries from an intermediary data structure. </summary>
	public static void ImportConfig(in ConfigExport export, bool intoLocal)
	{
		EnsureInitialized();

		LoadingEntryContext loadingEntryContext;
		loadingEntryContext.FileInfo = export;

		var entriesByName = ConfigSystem.EntriesByName;

		foreach (var pair in export.ValuesByEntry) {
			object value = pair.Value;

			if (!entriesByName.TryGetValue(pair.Key, out var entry)) {
				continue;
			}

			loadingEntryContext.Entry = entry;
			loadingEntryContext.Value = value;

			if (intoLocal && ShouldResetEntry(in loadingEntryContext)) {
				entry.LocalValue = entry.DefaultValue;
				continue;
			}

			if (intoLocal) {
				entry.LocalValue = value;
			} else {
				entry.RemoteValue = value;
			}
		}
	}

	/// <summary> Exports config entries' values to an intermediary data structure. </summary>
	public static void ExportConfig(out ConfigExport export, bool fromLocal)
	{
		EnsureInitialized();

		export.ModVersion = OverhaulMod.Instance.Version;
		export.ValuesByEntry = new();

		foreach (var entry in ConfigSystem.Entries) {
			export.ValuesByEntry[entry.Name] = (fromLocal ? entry.LocalValue : entry.RemoteValue) ?? entry.DefaultValue;
		}
	}

	/// <summary> Reads intermediary configuration values from a filesystem location using the provided interface. </summary>
	public static Result ReadConfig(string filePath, in ConfigFormat format, out ConfigExport export)
	{
		EnsureInitialized();

		if (!File.Exists(filePath)) {
			export = default;
			return Result.FileMissing;
		}

		if (!IOUtils.TryOperation(() => File.OpenRead(filePath), out var stream)) {
			export = default;
			return Result.FileInaccessible;
		}

		using (stream) {
			return format.ReadConfig(stream, out export);
		}
	}

	/// <summary> Writes intermediary configuration values into a filesystem location using the provided interface. </summary>
	public static Result WriteConfig(string filePath, in ConfigFormat format, in ConfigExport export)
	{
		EnsureInitialized();

		// Some writers utilize localization strings.
		TextSystem.ForceInitializeLocalization();

		if (!IOUtils.TryOperation(() => File.OpenWrite(filePath), out var stream)) {
			return Result.FileInaccessible;
		}

		using (stream) {
			var result = format.WriteConfig(stream, in export);

			stream.SetLength(stream.Position);

			return result;
		}
	}

	/// <summary> Reads & imports the main config file. </summary>
	public static Result LoadConfig()
	{
		EnsureInitialized();

		var config = default(ConfigExport);
		var result = default(Result);
		var formats = Formats;
		string basePath = FilePathWithoutExtension;

		for (int i = 0; i < formats.Length; i++) {
			string path = basePath + formats[i].Extension;

			result = ReadConfig(path, in formats[i], out config);

			if (result != Result.FileMissing) {
				if (i != 0) {
					using var _ = new Logging.QuietExceptionHandle();
					try { File.Delete(path); }
					catch { }
				}
				
				break;
			}
		}

		if (config.ModVersion == null && result == Result.Success) {
			result = Result.ModVersionMissing;
		}

		bool importConfig = true;

		if (result.HasFlag(Result.ErrorFlag)) {
			importConfig = false;
			DebugSystem.Logger.Error($"ConfigIO.LoadConfig(): {result}");
		} else if (result.HasFlag(Result.WarningFlag)) {
			DebugSystem.Logger.Warn($"ConfigIO.LoadConfig(): {result}");
		} else {
			DebugSystem.Logger.Info($"ConfigIO.LoadConfig(): {result}");
		}

		if (importConfig) {
			ImportConfig(in config, intoLocal: true);
			ConfigSynchronization.EnqueueSynchronization();
		}

		SaveConfig();

		return result;
	}

	/// <summary> Exports & writes the main config file. </summary>
	public static Result SaveConfig()
	{
		EnsureInitialized();

		try {
			ignoreConfigWatcherCounter++;
			lastConfigWatcherWriteTime = DateTime.Now.AddSeconds(0.5d); // Really ensure that this doesn't trigger an automatic config reload.

			ExportConfig(out var export, fromLocal: true);
			var result = WriteConfig(FilePathWithoutExtension + Formats[0].Extension, in Formats[0], in export);

			return result;
		}
		finally {
			ignoreConfigWatcherCounter--;
		}
	}

	private static void OnConfigDirectoryFileUpdateChanged(object sender, FileSystemEventArgs e)
	{
		// Only listen to changes to the config file itself.
		if (e.ChangeType != WatcherChangeTypes.Changed || e.FullPath != (FilePathWithoutExtension + Formats[0].Extension)) {
			return;
		}

		// Ignore changes to the config file if we're currently saving it, including cases triggered by refreshes.
		if (ignoreConfigWatcherCounter > 0) {
			return;
		}

		// Try to ignore repeating calls...
		DateTime lastWriteTime = File.GetLastWriteTime(e.FullPath);

		if (lastWriteTime > lastConfigWatcherWriteTime) {
			lastConfigWatcherWriteTime = lastWriteTime;

			MessageUtils.NewText("Automatically reloading config file...", Color.Orange, logAsInfo: true);

			LoadConfig();

			if (!Main.dedServ) {
				var sound = Common.Magic.MagicWeapon.MagicBlastSound;

				try {
					SoundEngine.PlaySound(sound with { Volume = 0.33f });
				}
				catch { }
			}
		}
	}
}

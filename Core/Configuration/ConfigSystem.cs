using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Newtonsoft.Json.Linq;
using Terraria.ModLoader;
using TerrariaOverhaul.Core.Debugging;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Core.Configuration
{
	public sealed class ConfigSystem : ModSystem
	{
		// Overengineering at its finest
		[Flags]
		public enum LoadingResult : byte
		{
			SuccessFlag = 1,	// 00000001
			WarningFlag = 2,    // 00000010
			ErrorFlag = 4,      // 00000100

			// Successes
			Success = (1 << 3) | SuccessFlag,
			// Warnings
			HadErrors = (1 << 3) | WarningFlag,
			// Errors
			FileMissing = (1 << 3) | ErrorFlag,
			FileBroken = (2 << 3) | ErrorFlag,
			ModVersionMissing = (3 << 3) | ErrorFlag,
		}

		private class CategoryData
		{
			public readonly Dictionary<string, IConfigEntry> EntriesByName = new();
		}

		public static readonly string ConfigPath = Path.Combine(OverhaulMod.PersonalDirectory, "Config.json");

		private static readonly Dictionary<string, IConfigEntry> entriesByName = new();
		private static readonly Dictionary<string, CategoryData> categoriesByName = new();

		public override void OnModLoad()
		{
			ForceInitializeStaticConstructors();

			DebugSystem.Log("Initializing configuration...");

			foreach (var entry in entriesByName.Values) {
				entry.Initialize(Mod);
			}

			LoadConfig();
			SaveConfig();
		}

		public override void Unload()
		{

		}

		private void ForceInitializeStaticConstructors()
		{
			DebugSystem.Log($"Running static constructors of types that contain config entries...");

			var assembly = Assembly.GetExecutingAssembly();
			string assemblyName = assembly.GetName().Name ?? throw new InvalidOperationException("Executing assembly lacks a 'Name'.");

			foreach (var mod in ModLoader.Mods) {
				var modAssembly = mod.GetType().Assembly;

				if (mod != Mod && !modAssembly.GetReferencedAssemblies().Any(n => n.Name == assemblyName)) {
					continue;
				}

				foreach (var type in modAssembly.GetTypes()) {
					if (type.IsEnum) {
						continue;
					}

					var fields = type.GetFields(ReflectionUtils.AnyBindingFlags); // This will include backing fields of properties.

					if (fields.Any(f => f.FieldType.GetInterfaces().Contains(typeof(IConfigEntry)))) {
						RuntimeHelpers.RunClassConstructor(type.TypeHandle);
					}
				}
			}
		}

		public static (LoadingResult result, string resultMessage) LoadConfig(bool resetOnError = true)
		{
			var result = LoadConfigInner(out var configVersion);

			if (configVersion == null && !result.HasFlag(LoadingResult.ErrorFlag)) {
				result = LoadingResult.ModVersionMissing;
			}

			string? resultMessage = GetLoadingResultMessage(result);

			if (result.HasFlag(LoadingResult.ErrorFlag)) {
				DebugSystem.Logger.Error(resultMessage);

				if (resetOnError) {
					DebugSystem.Logger.Info("Resetting configuration...");
					ResetConfig();
				}
			} else if (result.HasFlag(LoadingResult.WarningFlag)) {
				DebugSystem.Logger.Warn(resultMessage);
			} else {
				DebugSystem.Logger.Info(resultMessage);
			}

			return (result, resultMessage);
		}

		public static void SaveConfig()
		{
			var jObject = new JObject {
				["Meta"] = new JObject {
					["ModVersion"] = OverhaulMod.Instance.Version.ToString(),
				}
			};

			foreach (var entry in entriesByName.Values.OrderBy(e => $"{e.Category}.{e.Name}")) {
				if (!jObject.TryGetValue(entry.Category, out var categoryToken)) {
					jObject[entry.Category] = categoryToken = new JObject();
				}

				categoryToken[entry.Name] = JToken.FromObject(entry.LocalValue ?? entry.DefaultValue);
			}

			File.WriteAllText(ConfigPath, jObject.ToString());
		}

		public static void ResetConfig()
		{
			foreach (var entry in entriesByName.Values) {
				entry.LocalValue = entry.DefaultValue;
			}
		}

		public static string GetLoadingResultMessage(LoadingResult result) => result switch {
			LoadingResult.Success => "Config file loaded successfully.",
			LoadingResult.FileMissing => "Config file was not found.",
			LoadingResult.FileBroken => "Config file was broken.",
			LoadingResult.ModVersionMissing => "Config file was out of date -- Mod version string is missing.",
			LoadingResult.HadErrors => "Config file contained incorrect values.",
			_ => string.Empty,
		};

		internal static void RegisterEntry(IConfigEntry entry)
		{
			entriesByName.Add(entry.Name, entry);

			if (!categoriesByName.TryGetValue(entry.Category, out var category)) {
				categoriesByName[entry.Category] = category = new();
			}

			category.EntriesByName.Add(entry.Name, entry);
		}

		private static LoadingResult LoadConfigInner(out Version? configVersion)
		{
			configVersion = null;

			try {
				if (!File.Exists(ConfigPath)) {
					return LoadingResult.FileMissing;
				}

				string text = File.ReadAllText(ConfigPath);
				var jsonObject = JObject.Parse(text);

				if (jsonObject == null) {
					return LoadingResult.FileBroken;
				}

				if (jsonObject.TryGetValue("Meta", out var metaToken) && metaToken is JObject metaObject) {
					if (metaObject.TryGetValue("ModVersion", out var modVersionToken) && modVersionToken is JValue { Value: string modVersionString }) {
						if (Version.TryParse(modVersionString, out var version)) {
							configVersion = version;
						}
					}
				}

				bool hadErrors = false;

				foreach (var categoryPair in jsonObject) {
					if (categoryPair.Value is not JObject categoryJson) {
						continue;
					}

					if (!categoriesByName.TryGetValue(categoryPair.Key, out var category)) {
						continue;
					}

					foreach (var entryPair in categoryJson) {
						if (!category.EntriesByName.TryGetValue(entryPair.Key, out var entry)) {
							continue;
						}

						object? value = entryPair.Value?.ToObject(entry.ValueType);

						if (value != null) {
							entry.LocalValue = value;
						} else {
							hadErrors = true;
						}
					}
				}

				return hadErrors ? LoadingResult.HadErrors : LoadingResult.SuccessFlag;
			}
			catch {
				return LoadingResult.FileBroken;
			}
		}
	}
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Terraria.ModLoader;
using TerrariaOverhaul.Core.Debugging;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Core.Configuration;

public sealed partial class ConfigSystem : ModSystem
{
	public class CategoryData
	{
		public readonly Dictionary<string, IConfigEntry> EntriesByName = new();
	}

	public static readonly string ConfigDirectory = OverhaulMod.PersonalDirectory;
	public static readonly string ConfigPath = Path.Combine(OverhaulMod.PersonalDirectory, "Config.json");

	private static readonly Dictionary<string, IConfigEntry> entriesByName = new();
	private static readonly Dictionary<string, CategoryData> categoriesByName = new();

	private static FileSystemWatcher? configWatcher;
	private static DateTime lastConfigWatcherWriteTime;
	private static volatile int ignoreConfigWatcherCounter;

	public static IReadOnlyDictionary<string, IConfigEntry> EntriesByName { get; } = new ReadOnlyDictionary<string, IConfigEntry>(entriesByName);
	public static IReadOnlyDictionary<string, CategoryData> CategoriesByName { get; } = new ReadOnlyDictionary<string, CategoryData>(categoriesByName);

	public override void OnModLoad()
	{
		ForceInitializeStaticConstructors();

		DebugSystem.Log("Initializing configuration...");

		foreach (var entry in entriesByName.Values) {
			entry.Initialize(Mod);
		}

		InitializeIO();
		InitializeNetworking();

		LoadConfig();
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

	public static void ResetConfig()
	{
		foreach (var entry in entriesByName.Values) {
			entry.LocalValue = entry.DefaultValue;
		}
	}

	internal static void RegisterEntry(IConfigEntry entry)
	{
		entriesByName.Add(entry.Name, entry);

		if (!categoriesByName.TryGetValue(entry.Category, out var category)) {
			categoriesByName[entry.Category] = category = new();
		}

		category.EntriesByName.Add(entry.Name, entry);
	}
}

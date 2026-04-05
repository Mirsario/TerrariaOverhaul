// Copyright (c) 2020-2026 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;
using TerrariaOverhaul.Core.Debugging;

namespace TerrariaOverhaul.Core.Configuration;

internal enum ConfigSide
{
	Both,
	ClientOnly,
	ServerOnly
}

internal interface IConfigEntry
{
	Type ValueType { get; }
	bool IsHidden { get; }
	string Name { get; }
	string Category { get; }
	ReadOnlySpan<string> Categories { get; }
	object? Value { get; set; }
	object? LocalValue { get; set; }
	object? RemoteValue { get; set; }
	object DefaultValue { get; }
	ConfigSide Side { get; }
	LocalizedText? DisplayName { get; }
	LocalizedText? Description { get; }

	// Dumb.
	void Initialize(Mod mod, string? nameFallback = null);

	void Modified();
}

internal sealed class ConfigSystem : ModSystem
{
	public class CategoryData
	{
		public readonly Dictionary<string, IConfigEntry> EntriesByName = new();
	}

	private static readonly List<IConfigEntry> entries = new();
	private static readonly Dictionary<string, IConfigEntry> entriesByName;
	private static readonly Dictionary<string, CategoryData> categoriesByName;

	public static ReadOnlySpan<IConfigEntry> Entries => CollectionsMarshal.AsSpan(entries);
	public static ReadOnlyDictionary<string, IConfigEntry> EntriesByName { get; }
	public static ReadOnlyDictionary<string, CategoryData> CategoriesByName { get; }

	static ConfigSystem()
	{
		EntriesByName = new(entriesByName = new());
		CategoriesByName = new(categoriesByName = new());
	}

	public override void Load()
	{
		DebugSystem.Log("Initializing configuration...");

		ForceInitializeStaticConstructors();

		ConfigIO.LoadConfig();
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

			foreach (var type in AssemblyManager.GetLoadableTypes(modAssembly)) {
				if (type.IsEnum) {
					continue;
				}

				var fields = type.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
				bool ranStaticConstructor = false;

				foreach (var field in fields) {
					if (!typeof(IConfigEntry).IsAssignableFrom(field.FieldType)) {
						continue;
					}

					if (!ranStaticConstructor) {
						RuntimeHelpers.RunClassConstructor(type.TypeHandle);
						ranStaticConstructor = true;
					}

					RegisterEntry(mod, field);
				}
			}
		}
	}

	public static void ResetConfig()
	{
		DebugSystem.Logger.Info("Resetting configuration...");

		foreach (var entry in entries) {
			entry.LocalValue = entry.DefaultValue;
		}
	}

	private static void RegisterEntry(Mod mod, FieldInfo field)
	{
		var entry = (IConfigEntry)field.GetValue(null)!;

		RegisterEntry(mod, entry, nameFallback: field.Name);
	}

	private static void RegisterEntry(Mod mod, IConfigEntry entry, string? nameFallback = null)
	{
		entry.Initialize(mod, nameFallback);

		entries.Add(entry);
		entriesByName.Add(entry.Name, entry);

		void AddToCategory(string category)
		{
			if (!categoriesByName.TryGetValue(category, out var categoryData)) {
				categoriesByName[category] = categoryData = new();
			}

			categoryData.EntriesByName.Add(entry.Name, entry);
		}

		foreach (string category in entry.Categories) {
			AddToCategory(category);
		}
	}
}

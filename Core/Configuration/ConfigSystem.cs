﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Terraria.ModLoader;
using TerrariaOverhaul.Core.Debugging;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Core.Configuration;

public sealed class ConfigSystem : ModSystem
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
		ForceInitializeStaticConstructors();

		DebugSystem.Log("Initializing configuration...");

		foreach (var entry in entriesByName.Values) {
			entry.Initialize(Mod);
		}

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

			foreach (var type in modAssembly.GetTypes()) {
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

					RegisterEntry((IConfigEntry)field.GetValue(null)!);
				}
			}
		}
	}

	public static void ResetConfig()
	{
		DebugSystem.Logger.Info("Resetting configuration...");

		foreach (var entry in entriesByName.Values) {
			entry.LocalValue = entry.DefaultValue;
		}
	}

	internal static void RegisterEntry(IConfigEntry entry)
	{
		entries.Add(entry);
		entriesByName.Add(entry.Name, entry);

		void AddToCategory(string category)
		{
			if (!categoriesByName.TryGetValue(category, out var categoryData)) {
				categoriesByName[category] = categoryData = new();
			}

			categoryData.EntriesByName.Add(entry.Name, entry);
		}

		AddToCategory(entry.Category);

		foreach (string extraCategory in entry.ExtraCategories) {
			AddToCategory(extraCategory);
		}
	}
}

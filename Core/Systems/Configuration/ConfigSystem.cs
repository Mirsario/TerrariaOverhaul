using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;
using Terraria.ModLoader;
using TerrariaOverhaul.Core.Systems.Debugging;

namespace TerrariaOverhaul.Core.Systems.Configuration
{
	public sealed class ConfigSystem : ModSystem
	{
		internal class CategoryData
		{
			public readonly Dictionary<string, IConfigEntry> EntriesByName = new();
		}

		public static readonly string ConfigPath = Path.Combine(OverhaulMod.PersonalDirectory, "Config.json");

		private static readonly Dictionary<string, IConfigEntry> EntriesByName = new();
		private static readonly Dictionary<string, CategoryData> CategoriesByName = new();

		public override void OnModLoad()
		{
			foreach(var entry in EntriesByName.Values) {
				entry.Initialize(Mod);
			}

			if(!LoadConfig()) {
				DebugSystem.Logger.Warn("Config file contained incorrect values.");

			}
			
			SaveConfig();
		}

		public override void Unload()
		{
			
		}

		public override void SetupContent()
		{
			
		}

		public static bool LoadConfig()
		{
			if(!File.Exists(ConfigPath)) {
				return false;
			}

			string text = File.ReadAllText(ConfigPath);
			var jsonObject = JObject.Parse(text);

			if(jsonObject == null) {
				return false;
			}

			bool hadErrors = false;

			foreach(var categoryPair in jsonObject) {
				if(categoryPair.Value is not JObject categoryJson) {
					continue;
				}

				if(!CategoriesByName.TryGetValue(categoryPair.Key, out var category)) {
					continue;
				}

				foreach(var entryPair in categoryJson) {
					if(!category.EntriesByName.TryGetValue(entryPair.Key, out var entry)) {
						continue;
					}

					object value = entryPair.Value.ToObject(entry.ValueType);

					if(value != null) {
						entry.LocalValue = value;
					} else {
						hadErrors = true;
					}
				}
			}

			return !hadErrors;
		}

		public static void SaveConfig()
		{
			var jObject = new JObject();

			foreach(var entry in EntriesByName.Values) {
				if(!jObject.TryGetValue(entry.Category, out var categoryToken)) {
					jObject[entry.Category] = categoryToken = new JObject();
				}

				categoryToken[entry.Name] = JToken.FromObject(entry.LocalValue);
			}

			File.WriteAllText(ConfigPath, jObject.ToString());
		}

		internal static void RegisterEntry(IConfigEntry entry)
		{
			EntriesByName.Add(entry.Name, entry);

			if(!CategoriesByName.TryGetValue(entry.Category, out var category)) {
				CategoriesByName[entry.Category] = category = new();
			}

			category.EntriesByName.Add(entry.Name, entry);
		}
	}
}

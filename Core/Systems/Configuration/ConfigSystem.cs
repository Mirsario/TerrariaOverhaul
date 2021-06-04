using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Terraria.ModLoader;

namespace TerrariaOverhaul.Core.Systems.Configuration
{
	public sealed class ConfigSystem : ModSystem
	{
		public static readonly string ConfigPath = Path.Combine(OverhaulMod.PersonalDirectory, "Config.json");

		internal static readonly Dictionary<string, IConfigEntry> ConfigEntries = new();

		public override void OnModLoad()
		{
			foreach(var entry in ConfigEntries.Values) {
				entry.Initialize(Mod);
			}

			if(!LoadConfig()) {
				SaveConfig();
			}
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

			bool hadErrors = true;

			/*foreach(var token in jsonObject) {
				if(!(token.Value is JObject jObject)) {
					continue;
				}

				if(!ConfigsByName.TryGetValue(token.Key, out var config)) {
					continue;
				}

				var result = (Config)jObject.ToObject(config.GetType());

				if(result != null) {
					config.Local = result;
				} else {
					hadErrors = true;
				}
			}*/

			return !hadErrors;
		}

		public static void SaveConfig()
		{
			var jObject = new JObject();

			/*foreach(var config in Configs) {
				jObject[config.Name] = JObject.FromObject(config);
			}*/

			foreach(var entry in ConfigEntries.Values) {
				jObject[entry.Id] = JToken.FromObject(entry.LocalValue);
			}

			File.WriteAllText(ConfigPath, jObject.ToString());
		}
	}
}

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
		/*private static class ConfigInstance<T> where T : Config
		{
			public static T localInstance;
			public static T serverInstance;

			static ConfigInstance() => OnUnload += () => {
				localInstance = null;
				serverInstance = null;
			};
		}*/

		public static readonly string ConfigPath = Path.Combine(OverhaulMod.PersonalDirectory, "Config.json");

		internal static readonly List<Config> Configs = new List<Config>();
		internal static readonly Dictionary<string, Config> ConfigsByName = new Dictionary<string, Config>();

		private static event Action OnUnload;

		public override void Unload()
		{
			Configs.Clear();
			ConfigsByName.Clear();
			OnUnload?.Invoke();
		}
		public override void SetupContent()
		{
			foreach(var baseConfig in Configs) {
				baseConfig.Local = (Config)baseConfig.Clone();
			}

			if(!LoadConfig()) {
				SaveConfig();
			}
		}

		public static T GetConfig<T>(ConfigType type) where T : Config
		{
			var instance = ModContent.GetInstance<T>();

			switch(type) {
				case ConfigType.Local:
					return (T)(instance.Local);
				case ConfigType.Current:
					return (T)(instance.Server ?? instance.Local);
			}

			throw new ArgumentException();
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

			foreach(var token in jsonObject) {
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
			}

			return !hadErrors;
		}
		public static void SaveConfig()
		{
			var jObject = new JObject();

			foreach(var config in Configs) {
				jObject[config.Name] = JObject.FromObject(config);
			}

			File.WriteAllText(ConfigPath, jObject.ToString());
		}
	}
}

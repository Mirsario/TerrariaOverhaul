using System;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using IOResult = TerrariaOverhaul.Core.Configuration.ConfigIO.Result;

namespace TerrariaOverhaul.Core.Configuration;

public static class JsonConfig
{
	public static readonly ConfigFormat Format;

	static JsonConfig()
	{
		Format.ReadConfig = ReadConfig;
		Format.WriteConfig = WriteConfig;
		Format.Extension = ".json";
	}

	public static IOResult WriteConfig(Stream stream, in ConfigExport configExport)
	{
		var jObject = new JObject();

		if (configExport.ModVersion is Version modVersion) {
			jObject["Meta"] = new JObject {
				["ModVersion"] = modVersion.ToString(),
			};
		}

		var entriesByName = ConfigSystem.EntriesByName;
		var valuesByEntry = configExport.ValuesByEntry;

		foreach (var entry in entriesByName.Values.OrderBy(e => $"{e.Category}.{e.Name}")) {
			if (!valuesByEntry.TryGetValue(entry.Name, out object? value)) {
				continue;
			}

			if (!jObject.TryGetValue(entry.Category, out var categoryToken)) {
				jObject[entry.Category] = categoryToken = new JObject();
			}

			categoryToken[entry.Name] = JToken.FromObject(value);
		}

		string json = jObject.ToString(Formatting.Indented);
		byte[] data = Encoding.UTF8.GetBytes(json);

		stream.Write(data);

		return IOResult.Success;
	}

	public static IOResult ReadConfig(Stream stream, out ConfigExport configExport)
	{
		using var streamReader = new StreamReader(stream);
		using var jsonReader = new JsonTextReader(streamReader);

		try {
			var jsonObject = JObject.Load(jsonReader);

			if (jsonObject == null) {
				configExport = default;
				return IOResult.FileBroken;
			}

			configExport.ModVersion = null;

			if (jsonObject.TryGetValue("Meta", out var metaToken) && metaToken is JObject metaObject) {
				if (metaObject.TryGetValue("ModVersion", out var modVersionToken) && modVersionToken is JValue { Value: string modVersionString }) {
					if (Version.TryParse(modVersionString, out var version)) {
						configExport.ModVersion = version;
					}
				}
			}

			bool hadErrors = false;
			var categoriesByName = ConfigSystem.CategoriesByName;

			configExport.ValuesByEntry = new();

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
						configExport.ValuesByEntry[entry.Name] = value;
					} else {
						hadErrors = true;
					}
				}
			}

			return hadErrors ? IOResult.HadErrors : IOResult.Success;
		}
		catch {
			configExport = default;
			return IOResult.FileBroken;
		}
	}
}

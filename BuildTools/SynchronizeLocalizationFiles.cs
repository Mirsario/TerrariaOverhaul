using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Hjson;
using Microsoft.Build.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TerrariaOverhaul.BuildTools.Utilities;

namespace TerrariaOverhaul.BuildTools;

/// <summary>
/// <br/> Synchronizes HJSON localization files' keys with the provided default file.
/// <br/> This task does not create new localization keys based on declared content, only based on the keys declared in the fallback file.
/// </summary>
public sealed class SynchronizeLocalizationFiles : TaskBase
{
	private record class RecursionData
	{
		public CodeWriter Code { get; }
		public JObject Translation { get; }

		public RecursionData(CodeWriter code, JObject translation)
		{
			Code = code;
			Translation = translation;
		}
	}
	
	[Required]
	public string MainFile { get; set; } = string.Empty;

	[Required]
	public string[] LocalizationFiles { get; set; } = Array.Empty<string>();

	protected override void Run()
	{
		var baseTranslation = ReadHjsonFile(MainFile);
		var results = new Dictionary<string, RecursionData>();

		foreach (string translationFilePath in LocalizationFiles) {
			string cultureName = Path.GetFileNameWithoutExtension(translationFilePath);

			var code = new CodeWriter();
			var translation = ReadHjsonFile(translationFilePath);
			var data = new RecursionData(code, translation);

			Recursion(data, baseTranslation, isRoot: true);

			File.WriteAllText(translationFilePath, code.ToString());

			results[cultureName] = data;
		}
	}

	private static JObject ReadHjsonFile(string fileName)
	{
		string hjsonText = File.ReadAllText(fileName);
		using var hjsonReader = new StringReader(hjsonText);

		string jsonText = HjsonValue.Load(hjsonReader).ToString();
		var jsonObject = JObject.Parse(jsonText);

		return jsonObject;
	}

	private static void Recursion(RecursionData data, JToken token, string? linePrefix = null, bool isRoot = false)
	{
		var code = data.Code;
		var translation = data.Translation;
		var translatedToken = translation.SelectToken(token.Path);

		if (translatedToken == null) {
			linePrefix ??= "//";
		}

		switch (token) {
			case JProperty jsonProperty:
				if (jsonProperty.Value.Type != JTokenType.Object) {
					code.Write(linePrefix);
				}

				code.Write($"{jsonProperty.Name}:");

				Recursion(data, jsonProperty.Value);

				code.WriteLine();
				break;
			case JObject jsonObject:
				if (!isRoot) {
					code.WriteLine(" {");
					code.Indent();
				}

				var properties = jsonObject.Properties();
				var orderedValueProperties = properties.Where(p => p.Value.Type != JTokenType.Object).ToArray();
				var orderedObjectProperties = properties.Where(p => p.Value.Type == JTokenType.Object).ToArray();

				void RecurseProperties(JProperty[] properties, bool separateEntries = false)
				{
					for (int i = 0; i < properties.Length; i++) {
						Recursion(data, properties[i], linePrefix);

						if (separateEntries && i != properties.Length - 1) {
							code!.WriteLine();
						}
					}
				}

				RecurseProperties(orderedValueProperties);

				if (orderedValueProperties.Length != 0 && orderedObjectProperties.Length != 0) {
					code.WriteLine();
				}

				RecurseProperties(orderedObjectProperties, separateEntries: true);

				if (!isRoot) {
					code.Unindent();
					code.Write("}");
				}

				break;
			case JValue jsonValue:
				string jsonValueText = (translatedToken as JValue ?? jsonValue).ToString();

				if (!jsonValueText.Contains('\n')) {
					if (jsonValue.Type == JTokenType.String) {
						code.Write($@" ""{jsonValueText}""");
					} else {
						code.Write($" {jsonValueText}");
					}

					break;
				}

				string[] split = jsonValueText.Replace("\r\n", "\n").Split('\n');

				code.WriteLine();
				code.WriteLine($"{linePrefix}\t'''");

				foreach (string portion in split) {
					code.WriteLine($"{linePrefix}\t{portion}");
				}

				code.Write($"{linePrefix}\t'''");

				break;
			default:
				code.Write($"/* Unhandled token: {token.GetType()} */");
				break;
		}
	}
}

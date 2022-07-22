using System;
using System.IO;
using System.Linq;
using Hjson;
using Microsoft.Build.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TerrariaOverhaul.BuildTools.Utilities;

namespace TerrariaOverhaul.BuildTools
{
	/// <summary>
	/// Formats and populates HJSON localization files based on a default fallback file.
	/// </summary>
	public class UpdateLocalizationFiles : TaskBase
	{
		private class RecursionParameters
		{
			public CodeWriter Code { get; }
			public JObject Translation { get; }

			public RecursionParameters(CodeWriter code, JObject translation)
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

			foreach (string translationFilePath in LocalizationFiles) {
				var code = new CodeWriter();
				var translation = ReadHjsonFile(translationFilePath);
				var parameters = new RecursionParameters(code, translation);

				Recursion(parameters, baseTranslation, isRoot: true);

				File.WriteAllText(translationFilePath, code.ToString());
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

		private static void Recursion(RecursionParameters parameters, JToken token, string? linePrefix = null, bool isRoot = false)
		{
			var code = parameters.Code;
			var translation = parameters.Translation;
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

					Recursion(parameters, jsonProperty.Value);

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
							Recursion(parameters, properties[i], linePrefix);

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
}

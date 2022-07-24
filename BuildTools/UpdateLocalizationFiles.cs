using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
		private class RecursionData
		{
			public CodeWriter Code { get; }
			public JObject Translation { get; }
			public int PresentTranslationCount { get; set; }
			public int MissingTranslationCount { get; set; }
			
			public int TotalTranslationCount => PresentTranslationCount + MissingTranslationCount;

			public RecursionData(CodeWriter code, JObject translation)
			{
				Code = code;
				Translation = translation;
			}
		}
		
		public string ResultsOutputPath { get; set; } = string.Empty;

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

			if (!string.IsNullOrWhiteSpace(ResultsOutputPath)) {
				string usedPath = Path.ChangeExtension(ResultsOutputPath, ".md");
				var resultsText = new StringBuilder();

				const string Header = "# Results of the last localization refresh";

				resultsText.AppendLine(Header);
				resultsText.AppendLine();

				foreach (var pair in results) {
					string cultureName = pair.Key;
					var data = pair.Value;

					string status = data.PresentTranslationCount != 0 ? (data.MissingTranslationCount == 0 ? "✅ Full!" : "⚠️ Incomplete!") : "❌ Not even started!";

					resultsText.AppendLine($"## {cultureName}");
					resultsText.AppendLine($"- **Status:** {status}");
					resultsText.AppendLine($"- **Completion:** ***{data.PresentTranslationCount / (float)data.TotalTranslationCount * 100f:0.0}%***");
					resultsText.AppendLine($"- **Translated:** `{data.PresentTranslationCount}` out of `{data.TotalTranslationCount}` (`{data.MissingTranslationCount}` missing!)");
					resultsText.AppendLine();
				}

				string finalizedResultsText = resultsText.ToString();

				if (File.Exists(usedPath)) {
					string existingText = File.ReadAllText(usedPath);
					int headerIndex = existingText.IndexOf(Header);

					if (headerIndex >= 0) {
						finalizedResultsText = $"{existingText.Substring(0, headerIndex)}{finalizedResultsText}";
					}
				}

				File.WriteAllText(usedPath, finalizedResultsText);
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
						if (translatedToken == null) {
							data.MissingTranslationCount++;
						} else {
							data.PresentTranslationCount++;
						}

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
}

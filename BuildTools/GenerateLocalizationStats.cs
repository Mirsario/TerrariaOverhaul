using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Hjson;
using Microsoft.Build.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TerrariaOverhaul.BuildTools;

/// <summary>
/// Generates a markdown file containing completion percentages of translation files.
/// </summary>
public sealed class GenerateLocalizationStats : TaskBase
{
	private class RecursionData
	{
		public JObject Translation { get; }
		public int PresentTranslationCount { get; set; }
		public int MissingTranslationCount { get; set; }
		
		public int TotalTranslationCount => PresentTranslationCount + MissingTranslationCount;

		public RecursionData(JObject translation)
		{
			Translation = translation;
		}
	}
	
	[Required]
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

			var translation = ReadHjsonFile(translationFilePath);
			var data = new RecursionData(translation);

			Recursion(data, baseTranslation);

			results[cultureName] = data;
		}

		if (!string.IsNullOrWhiteSpace(ResultsOutputPath)) {
			string usedPath = Path.ChangeExtension(ResultsOutputPath, ".md");
			var resultsText = new StringBuilder();

			const string Header = "# Results of the last localization refresh";

			// Prevent commas from being used in place of periods.
			// We don't want that to appear in PRs.
			CultureInfo.CurrentCulture = new CultureInfo("en-US");

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

	private static void Recursion(RecursionData data, JToken token)
	{
		var translation = data.Translation;
		var translatedToken = translation.SelectToken(token.Path);

		switch (token) {
			case JProperty jsonProperty:
				if (jsonProperty.Value.Type != JTokenType.Object) {
					if (translatedToken == null) {
						data.MissingTranslationCount++;
					} else {
						data.PresentTranslationCount++;
					}
				}

				Recursion(data, jsonProperty.Value);
				break;
			case JObject jsonObject:
				var properties = jsonObject.Properties();

				foreach (var property in properties) {
					Recursion(data, property);
				}

				break;
		}
	}
}

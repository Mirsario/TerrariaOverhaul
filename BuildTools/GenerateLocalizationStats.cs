// Copyright (c) 2020-2026 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Transactions;
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
	private class Stats
	{
		public int PresentTranslationCount { get; set; }
		public int MissingTranslationCount { get; set; }
		
		public int TotalTranslationCount => PresentTranslationCount + MissingTranslationCount;
	}

	private static readonly Regex culturePrefixRegex = new(@"([a-z][a-z]-[A-Za-z]+)[\w/\\]*?(?:_?((?:\w+\.?)+))?\.hjson");
	private static readonly string[] knownCultures = {
		"de-DE", "es-ES", "fr-FR", "it-IT", "pl-PL", "pt-BR", "ru-RU", "zh-Hans",
	};
	
	[Required]
	public string ResultsOutputPath { get; set; } = string.Empty;

	[Required]
	public string MainCulture { get; set; } = string.Empty;

	[Required]
	public string[] LocalizationFiles { get; set; } = Array.Empty<string>();

	protected override void Run()
	{
		var localizationsByCulture = new Dictionary<string, JObject>();

		foreach (string path in LocalizationFiles) {
			if (!TryGetCultureAndPrefixFromPath(path, out string cultureName, out string prefix)) {
				throw new InvalidOperationException($"Path not parsed: {path}");
			}

			var translation = ReadHjsonFile(path);

			if (!string.IsNullOrWhiteSpace(prefix)) {
				string[] keys = prefix.Split('.');

				for (int i = keys.Length - 1; i >= 0; i--) {
					translation = new JObject {
						[keys[i]] = translation
					};
				}
			}

			if (!localizationsByCulture.TryGetValue(cultureName, out var json)) {
				localizationsByCulture[cultureName] = translation;
			} else {
				json.Merge(translation);
			}
		}

		if (!localizationsByCulture.TryGetValue(MainCulture, out var baseLocalization)) {
			throw new InvalidOperationException($"No main culture files found!\r\nAll keys: {string.Join(", ", localizationsByCulture.Keys)}");
		}

		var results = new Dictionary<string, Stats>();

		foreach (var pair in localizationsByCulture) {
			if (!results.TryGetValue(pair.Key, out var stats)) {
				results[pair.Key] = stats = new Stats();
			}

			Recursion(pair.Value, stats, baseLocalization);
		}

		WriteResults(results);
	}

	private void WriteResults(Dictionary<string, Stats> results)
	{
		if (string.IsNullOrWhiteSpace(ResultsOutputPath)) {
			return;
		}

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

	private static JObject ReadHjsonFile(string filePath)
	{
		string hjsonText = File.ReadAllText(filePath);
		using var hjsonReader = new StringReader(hjsonText);

		string jsonText = HjsonValue.Load(hjsonReader).ToString();
		var jsonObject = JObject.Parse(jsonText);

		return jsonObject;
	}

	private static void Recursion(JObject translation, Stats stats, JToken token)
	{
		var translatedToken = translation.SelectToken(token.Path);

		switch (token) {
			case JProperty jsonProperty:
				if (jsonProperty.Value.Type != JTokenType.Object) {
					if (translatedToken == null) {
						stats.MissingTranslationCount++;
					} else {
						stats.PresentTranslationCount++;
					}
				}

				Recursion(translation, stats, jsonProperty.Value);
				break;
			case JObject jsonObject:
				var properties = jsonObject.Properties();

				foreach (var property in properties) {
					Recursion(translation, stats, property);
				}

				break;
		}
	}

	private static bool TryGetCultureAndPrefixFromPath(string path, out string culture, out string prefix)
	{
		var match = culturePrefixRegex.Match(path);

		if (match.Success) {
			culture = match.Groups[1].Value;
			prefix = match.Groups[2].Value;
			return true;
		}

		culture = default!;
		prefix = default!;
		return false;
	}
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using TerrariaOverhaul.Core.Localization;

namespace TerrariaOverhaul.Common.Bosses;

public sealed class BossLines : ModSystem
{
	private struct Common()
	{
		public string[][] DefeatLines = [];
	}
	public struct Specific()
	{
		public string Name;
		public string[] SubTitles = [];
		public string[] DefeatLines = [];
		public int GenderId;
	}

	private const string BaseKey = $"Mods.{nameof(TerrariaOverhaul)}.Bosses";
	private static Common commons;
	private static int lastLanguageRefreshCount;
	private static readonly Dictionary<int, Specific?> cache = new();

	static BossLines()
	{
#if DEBUG
		string[] input = ["a;b;c", "d;e;f"];
		string[][] expect = [["a", "d"], ["b", "e"], ["c", "f"]];
		string[][] actual = ParseGenderedLines(input);
		Debug.Assert(expect.Length == actual.Length);
		Debug.Assert(expect[0].SequenceEqual(actual[0]));
		Debug.Assert(expect[1].SequenceEqual(actual[1]));
#endif
	}

	public override void OnModLoad() => UpdateCache();

	private static void UpdateCache()
	{
		if (lastLanguageRefreshCount == TextSystem.LanguageRefreshCount)
			return;

		cache.Clear();
		lastLanguageRefreshCount = TextSystem.LanguageRefreshCount;

		Common c;
		c.DefeatLines = ParseGenderedLines(GatherNumberedLines($"Mods.{nameof(TerrariaOverhaul)}.Bosses.Common.Defeated", 1));
		commons = c;
	}

	public static bool TryGet(int type, [MaybeNullWhen(false)] out Specific lines)
	{
		UpdateCache();

		if (cache.TryGetValue(type, out var linesOrNull)) {
			lines = linesOrNull ?? default;
			return linesOrNull.HasValue;
		}

		if (!NPCID.Search.TryGetName(type, out string idName)) {
			cache[type] = null;
			lines = default;
			return false;
		}

		string bossBaseKey = $"{BaseKey}.{idName}";
		lines.GenderId = 0;

		if (!TryGetTextValue($"{bossBaseKey}.Name", out lines.Name!)
		|| (TryGetTextValue($"{bossBaseKey}.GenderId", out var genderStr) && !int.TryParse(genderStr, out lines.GenderId))) {
			cache[type] = null;
			lines = default;
			return false;
		}

		lines.SubTitles = GatherNumberedLines($"{bossBaseKey}.SubLine", 1);
		lines.DefeatLines = commons.DefeatLines[lines.GenderId];

		cache[type] = lines;
		return true;
	}

	private static bool TryGetTextValue(string key, [NotNullWhen(true)] out string? value)
	{
		value = Language.GetTextValue(key);
		return value != null && value != key;
	}

	private static string[] GatherNumberedLines(string key, int startIndex)
	{
		int numLines = 0;
		for (int i = startIndex; TryGetTextValue($"{key}{i}", out _); i++) numLines++;
		if (numLines == 0) return [];

		var result = new string[numLines];
		for (int i = 0; i < numLines; i++) result[i] = Language.GetTextValue($"{key}{i + startIndex}");

		return result;
	}

	/// <summary> Converts <code>[ "a;b;c", "d;e;f" ]</code> into <code>[ [ "a", "d" ], [ "b", "e" ], [ "c", "f" ] ]</code> </summary>
	private static string[][] ParseGenderedLines(string[] lines)
	{
		const char SplitChar = ';';
		var results = new string[lines.Max(l => l.Count(c => c == SplitChar) + 1)][];

		for (int lineIndex = 0; lineIndex < lines.Length; lineIndex++) {
			var srcLine = lines[lineIndex];

			int languageIndex = 0;
			for (int spanSplit = 0; spanSplit < srcLine.Length; languageIndex++) {
				int nextSplit = srcLine.IndexOf(SplitChar, spanSplit) is >= 0 and int index ? index : srcLine.Length;
				results[languageIndex] ??= new string[lines.Length];
				results[languageIndex][lineIndex] = srcLine[spanSplit..nextSplit];
				spanSplit = nextSplit + 1;
			}
			// Spread last value if the list ends abruptly.
			for (int i = languageIndex; i < results.Length; i++) {
				results[i][lineIndex] = results[languageIndex][lineIndex];
			}
		}

		return results;
	}
}

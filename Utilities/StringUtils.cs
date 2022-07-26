using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;

namespace TerrariaOverhaul.Utilities;

internal static class StringUtils
{
	private static readonly Dictionary<string, string> regexCache = new();

	public static string? SafeFormat(string? str, object? arg0)
		=> str?.Replace("{0}", arg0?.ToString() ?? "");

	public static string? SafeFormat(string? str, object? arg0, object? arg1)
		=> str?.Replace("{0}", arg0?.ToString() ?? "").Replace("{1}", arg1?.ToString() ?? "");

	public static string? SafeFormat(string? str, params object[] args)
	{
		if (str != null) {
			for (int i = 0; i < args.Length; i++) {
				str = str.Replace("{" + i + "}", args[i]?.ToString() ?? "");
			}
		}

		return str;
	}

	public static string JoinLines(params string[] strings)
		=> string.Join("\r\n", strings);

	public static string ColoredText(Color color, string text)
		=> $"[c/{color.ToHexRGB()}:{text}]";

	public static string GetTextWithoutTags(string colorCodedText)
	{
		if (!regexCache.TryGetValue(colorCodedText, out string? plainText)) {
			regexCache[colorCodedText] = plainText = Regex.Replace(colorCodedText, @"\[\w\/\w+:([^\]]+)\]", "$1");
		}

		return plainText;
	}
}

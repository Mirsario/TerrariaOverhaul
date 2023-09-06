using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Terraria.ModLoader;
using TerrariaOverhaul.Core.Input;

namespace TerrariaOverhaul.Utilities;

public static class TooltipUtils
{
	private static readonly Regex combatInfoAccentRegex = new Regex(@"\[\[([\s\S]+?)\]\]", RegexOptions.Compiled);
	private static readonly Color combatInfoBaseColor = Color.LightSteelBlue;
	private static readonly Color combatInfoAccentColor = Color.MediumVioletRed;
	private static readonly Color combatInfoAccentColorAlt = ColorUtils.FromHexRgba(0xa1035200);

	public static bool ShowCombatInformation(Mod mod, List<TooltipLine> tooltips, Func<IEnumerable<string>> getLines)
	{
		const Keys Key = Keys.LeftAlt;

		void AddTooltip(string name, string text, Color color)
		{
			tooltips.Add(new TooltipLine(mod, name, text) {
				OverrideColor = color
			});
		}

		if (InputSystem.GetKey(Key)) {
			static string MatchEvaluator(Match match)
				=> StringUtils.ColoredText(combatInfoAccentColorAlt, match.Groups[1].Value);

			var lines = getLines().Select(s => combatInfoAccentRegex.Replace(s, MatchEvaluator));

			AddTooltip("CombatInfoSeparator", mod.GetTextValue("CommonTooltips.CombatInfo"), combatInfoAccentColor);
			AddTooltip("CombatInfo", string.Join("\r\n", lines), combatInfoBaseColor);

			return true;
		}

		AddTooltip("ShowCombatInfo", mod.GetTextValue("CommonTooltips.ShowCombatInfo").Replace("{Key}", Key.ToString()), combatInfoAccentColor);

		return false;
	}
}

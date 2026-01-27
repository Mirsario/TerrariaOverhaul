// Copyright (c) 2020-2026 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Terraria.ModLoader;
using TerrariaOverhaul.Core.Configuration;
using TerrariaOverhaul.Core.Input;
using TerrariaOverhaul.Utilities.Terraria;
using TerrariaOverhaul.Utilities.Xna;

#pragma warning disable IDE0044

namespace TerrariaOverhaul.Common.Items;

internal static partial class ItemTooltips
{
	public static readonly ConfigEntry<bool> ShowCombatInfoHintTooltip = new(ConfigSide.ClientOnly, true, "Interface");

	private static readonly Regex combatInfoAccentRegex = CombatInfoAccentRegex();
	private static readonly Color combatInfoBaseColor = Color.LightSteelBlue;
	private static readonly Color combatInfoAccentColor = Color.MediumVioletRed;
	private static readonly Color combatInfoAccentColorAlt = ColorUtils.FromHexRgba(0xa1035200);
	private static Keys combatInfoModifierKey = Keys.LeftAlt;

	public static bool ShowCombatInformation(Mod mod, List<TooltipLine> tooltips, Func<IEnumerable<string>> getLines)
	{
		var lines = getLines();
		if (!lines.Any()) {
			return false;
		}

		void AddTooltip(string name, string text, Color color)
		{
			tooltips.Add(new TooltipLine(mod, name, text) {
				OverrideColor = color
			});
		}

		if (InputSystem.GetKey(combatInfoModifierKey)) {
			static string MatchEvaluator(Match match)
				=> ChatTagUtils.ColoredText(combatInfoAccentColorAlt, match.Groups[1].Value);

			lines = lines.Select(s => "◙ " + combatInfoAccentRegex.Replace(s, MatchEvaluator).Replace("\n", "\n   "));

			AddTooltip("CombatInfoSeparator", mod.GetTextValue("CommonTooltips.CombatInfo"), combatInfoAccentColor);
			AddTooltip("CombatInfo", string.Join("\r\n", lines), combatInfoBaseColor);

			return true;
		}

		if (ShowCombatInfoHintTooltip) {
			string showCombatInfoText = mod.GetTextValue("CommonTooltips.ShowCombatInfo").Replace("{Key}", combatInfoModifierKey.ToString());
			AddTooltip("ShowCombatInfo", showCombatInfoText, combatInfoAccentColor);
		}

		return false;
	}

	[GeneratedRegex(@"\[\[([\s\S]+?)\]\]", RegexOptions.Compiled)]
	private static partial Regex CombatInfoAccentRegex();
}

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;
using MonoMod.Cil;
using Terraria;
using Terraria.ModLoader;
using TerrariaOverhaul.Core.Localization;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.CriticalStrikes;

[Autoload(Side = ModSide.Client)]
public sealed class CriticalStrikeTooltips : GlobalItem
{
	private static readonly int maxCacheEntries = 128;
	private static readonly Text expression = Text.Localized($"Mods.{nameof(TerrariaOverhaul)}.CommonTooltips.CriticalDamage.Expression");
	private static readonly Text replacement = Text.Localized($"Mods.{nameof(TerrariaOverhaul)}.CommonTooltips.CriticalDamage.Replacement");
	private static readonly Dictionary<string, string> cache = new();
	private static string? lastExpression;
	private static Regex? regex;

	private static Regex CritRegex {
		get {
			string text = expression.Value;
			
			if (regex == null || text != lastExpression) {
				lastExpression = text;
				regex = new Regex(text, RegexOptions.Compiled);
			}

			return regex;
		}
	}

	public override void Load()
	{
		MonoModHooks.Modify(typeof(ItemLoader).GetMethod(nameof(ItemLoader.ModifyTooltips))!, ModifyTooltipsInjection);
	}

	public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
	{
		if (tooltips.FirstOrDefault(t => t is { Mod: "Terraria", Name: "CritChance" }) is { } critTooltip) {
			using var _ = CriticalStrikeRework.AllowCritChanceReturn();

			int critChance = Main.LocalPlayer.GetWeaponCrit(item);
			int damage = Main.LocalPlayer.GetWeaponDamage(item, true);
			float critMult = CriticalStrikeRework.TotalCritChanceToDamageScale(critChance);
			int critDamage = (int)MathF.Round(damage * critMult);

			//var critString = $"{critDamage} (x{critMult:0.00})";
			var critString = $"x{critMult:0.00}";

			critTooltip.Text = Mod.GetTextValue("CommonTooltips.CriticalDamage.Tooltip", [critString]);
			critTooltip.OverrideColor = ColorUtils.FromHexRgb(0xc5aae4);
		}
	}

	private static void ModifyTooltipsInjection(ILContext context)
	{
		var il = new ILCursor(context);

		while (il.TryGotoNext(i => i.MatchRet())) { }

		il.EmitDelegate(static (List<TooltipLine> lines) => {
			if (cache.Count > maxCacheEntries) {
				cache.Clear();
			}

			foreach (var line in lines) {
				if (!cache.TryGetValue(line.Text, out string? newText)) {
					newText = CritRegex.Replace(line.Text, replacement);
					cache[line.Text] = newText;
				}

				line.Text = newText;
			}

			// Put lines back on the stack so that the Ret opcode still works.
			return lines;
		});
	}
}

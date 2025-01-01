using System;
using Terraria;
using Terraria.ModLoader;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.CriticalStrikes;

public sealed class CriticalStrikeRework : ModSystem
{
	private static Counter skipTotalCritCounter = default;

	public override void Load()
	{
		On_Player.GetWeaponCrit += GetWeaponCritInjection;
		On_Player.GetTotalCritChance += GetTotalCritChanceInjection;
		On_Main.MouseText_DrawItemTooltip_GetLinesInfo += GetLinesInfoInjection;
	}

	public static Counter.Handle AllowCritChanceReturn()
		=> skipTotalCritCounter.Increase();

	public static float CritChanceToScale(int chance)
		=> 1.2f + Math.Max(0, chance - 4) * 0.01f;

	// Force all crit chances to be zero.
	private static int GetWeaponCritInjection(On_Player.orig_GetWeaponCrit orig, Player player, Item sItem)
		=> skipTotalCritCounter.Active ? orig(player, sItem) : 0;
	private static float GetTotalCritChanceInjection(On_Player.orig_GetTotalCritChance orig, Player player, DamageClass damageClass)
		=> skipTotalCritCounter.Active ? orig(player, damageClass) : 0f;
	
	private static void GetLinesInfoInjection(On_Main.orig_MouseText_DrawItemTooltip_GetLinesInfo orig, Item item, ref int yoyoLogo, ref int researchLine, float oldKB, ref int numLines, string[] tooltipLine, bool[] prefixLine, bool[] badPrefixLine, string[] tooltipNames, out int prefixLineIndex)
	{
		using var _ = AllowCritChanceReturn();
		orig(item, ref yoyoLogo, ref researchLine, oldKB, ref numLines, tooltipLine, prefixLine, badPrefixLine, tooltipNames, out prefixLineIndex);
	}
}

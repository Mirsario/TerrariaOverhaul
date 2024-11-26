using Terraria;
using Terraria.ModLoader;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.Weakpoints;

public sealed class CriticalStrikeRework : ModSystem
{
	private static Counter skipTotalCritCounter = default;

	public override void Load()
	{
		On_Player.GetTotalCritChance += GetTotalCritChanceInjection;
		On_Player.GetWeaponCrit += GetWeaponCritInjection;
	}

	public static Counter.Handle AllowCritChanceReturn()
		=> skipTotalCritCounter.Increase();

	// Force all crit chances to be zero.
	private static int GetWeaponCritInjection(On_Player.orig_GetWeaponCrit orig, Player player, Item sItem)
	{
		return skipTotalCritCounter.Active ? orig(player, sItem) : 0;
	}
	private static float GetTotalCritChanceInjection(On_Player.orig_GetTotalCritChance orig, Player player, DamageClass damageClass)
	{
		return skipTotalCritCounter.Active ? orig(player, damageClass) : 0f;
	}
}

using Terraria;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.Charging;
using TerrariaOverhaul.Common.Items;
using TerrariaOverhaul.Core.ItemComponents;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.Archery;

internal class ItemPrimaryUseCharging : ItemComponent
{
	private Timer charge;

	public float UseLengthMultiplier { get; set; } = 0.5f;
	public float ChargeLengthMultiplier { get; set; } = 0.5f;

	public Timer Charge => charge;

	public override void Load()
	{
		On_Player.ItemCheck_CheckCanUse += PlayerCheckCanUseDetour;
	}

	public override void HoldItem(Item item, Player player)
	{
		if (charge.UnclampedValue == 0) {
			player.GetModPlayer<PlayerItemUse>().ForceItemUse();
		} else if (charge.UnclampedValue > 0) {
			ApplyDummyAnimationTime(player);
		}
	}

	public override float UseAnimationMultiplier(Item item, Player player)
	{
		if (charge.UnclampedValue == -1) {
			return UseLengthMultiplier;
		}

		return 1.0f;
	}

	public bool StartCharge(Item item, Player player)
	{
		if (charge.UnclampedValue == -1) {
			return false;
		}

		if (item.TryGetGlobalItem(out ItemPowerAttacks powerAttacks) && powerAttacks.PowerAttack) {
			return false;
		}

		uint length = (uint)CombinedHooks.TotalAnimationTime(item.useAnimation * ChargeLengthMultiplier, player, item);

		charge.Set(length);
		ApplyDummyAnimationTime(player);

		return true;
	}

	private void ApplyDummyAnimationTime(Player player)
	{
		player.itemTime = 2;
		player.itemAnimation = player.itemAnimationMax;
	}

	private static bool PlayerCheckCanUseDetour(On_Player.orig_ItemCheck_CheckCanUse orig, Player player, Item item)
	{
		if (orig(player, item)) {
			if (item.TryGetGlobalItem(out ItemPrimaryUseCharging charging) && charging.Enabled && charging.StartCharge(item, player)) {
				return false;
			}

			return true;
		}

		return false;
	}
}

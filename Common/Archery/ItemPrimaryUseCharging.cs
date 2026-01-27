// Copyright (c) 2020-2026 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using Terraria;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.Charging;
using TerrariaOverhaul.Common.Items;
using TerrariaOverhaul.Core.ItemComponents;
using TerrariaOverhaul.Utilities.Terraria;

namespace TerrariaOverhaul.Common.Archery;

internal class ItemPrimaryUseCharging : ItemComponent
{
	private GameTimer charge;

	public float UseLengthMultiplier { get; set; } = 0.5f;
	public float ChargeLengthMultiplier { get; set; } = 0.5f;

	public GameTimer Charge => charge;

	public override void Load()
	{
		On_Player.ItemCheck_CheckCanUse += PlayerCheckCanUseDetour;
	}

	public override void HoldItem(Item item, Player player)
	{
		if (!Enabled) {
			return;
		}

		if (charge.UnclampedUnfrozenValue == 0 && charge.CurrentTime != 0) {
			if (player.IsLocal()) {
				player.GetModPlayer<PlayerItemUse>().ForceItemUse();
			}
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

using Microsoft.Xna.Framework;
using Terraria;
using TerrariaOverhaul.Common.Charging;
using TerrariaOverhaul.Core.ItemComponents;
using TerrariaOverhaul.Core.Time;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.Melee;

public sealed class ItemMeleePowerAttackEffects : ItemComponent
{
	private Timer lastCharge;

	public override void HoldItem(Item item, Player player)
	{
		if (!Enabled) {
			return;
		}

		if (item.TryGetGlobalItem<ItemPowerAttacks>(out var powerAttacks) && item.TryGetGlobalItem<ItemMeleeAttackAiming>(out var aiming)) {
			var charge = powerAttacks.Charge;

			if (charge.Active) {
				// If a new charge was just started
				if (charge != lastCharge) {
					// Set attack direction
					aiming.AttackDirection = Vector2.UnitX * player.direction;

					// Force swing to be downwards
					if (item.TryGetGlobalItem(out QuickSlashMeleeAnimation anim)) {
						anim.IsAttackFlipped = false;
					}

					lastCharge = charge;
				} else {
					aiming.AttackDirection = Vector2.Lerp(aiming.AttackDirection, player.LookDirection(), 5f * TimeSystem.LogicDeltaTime);
				}
			}
		}
	}
}

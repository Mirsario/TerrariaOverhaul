using Microsoft.Xna.Framework;
using Terraria;
using TerrariaOverhaul.Common.ModEntities.Items.Components.Animations;
using TerrariaOverhaul.Core.Time;
using TerrariaOverhaul.Utilities.Extensions;

namespace TerrariaOverhaul.Common.ModEntities.Items.Components.Melee
{
	public sealed class ItemMeleePowerAttackEffects : ItemComponent
	{
		private bool initialized;

		// Whaaaaaat a craaap designnn
		public override void HoldItem(Item item, Player player)
		{
			if (!Enabled || initialized) {
				return;
			}

			if (item.TryGetGlobalItem<ItemPowerAttacks>(out var powerAttacks) && item.TryGetGlobalItem<ItemMeleeAttackAiming>(out _)) {
				powerAttacks.OnChargeStart += (item, player, chargeLength) => {
					if (item.TryGetGlobalItem(out ItemMeleeAttackAiming aiming)) {
						aiming.AttackDirection = Vector2.UnitX * player.direction;

						// Hardcode pretty much..
						if (item.TryGetGlobalItem(out QuickSlashMeleeAnimation anim)) {
							anim.IsAttackFlipped = false;
						}
					}
				};

				powerAttacks.OnChargeUpdate += (item, player, chargeLength, progress) => {
					if (item.TryGetGlobalItem(out ItemMeleeAttackAiming aiming)) {
						aiming.AttackDirection = Vector2.Lerp(aiming.AttackDirection, player.LookDirection(), 5f * TimeSystem.LogicDeltaTime);
					}
				};
			}

			initialized = true;
		}
	}
}

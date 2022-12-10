using Microsoft.Xna.Framework;
using Terraria;
using TerrariaOverhaul.Common.Hooks.Items;
using TerrariaOverhaul.Common.Melee;
using TerrariaOverhaul.Core.ItemComponents;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.BloodAndGore;

public sealed class ItemMeleeGoreInteraction : ItemComponent
{
	public override void HoldItem(Item item, Player player)
	{
		if (!Enabled) {
			return;
		}

		if (player.itemAnimation < player.itemAnimationMax - 1 || !ICanDoMeleeDamage.Invoke(item, player)) {
			return;
		}

		if (!item.TryGetGlobalItem(out ItemMeleeAttackAiming meleeAttackAiming)) {
			return;
		}

		const int MaxHits = 5;

		float range = ItemMeleeAttackAiming.GetAttackRange(item, player);
		float arcRadius = MathHelper.Pi * 0.5f;
		int numHit = 0;

		for (int i = 0; i < Main.maxGore; i++) {
			if (Main.gore[i] is not OverhaulGore gore || !gore.active || gore.Time < 30) {
				continue;
			}

			if (CollisionUtils.CheckRectangleVsArcCollision(gore.AABBRectangle, player.Center, meleeAttackAiming.AttackAngle, arcRadius, range)) {
				gore.ApplyForce(meleeAttackAiming.AttackDirection);

				if (gore.Damage()) {
					numHit++;

					if (numHit >= MaxHits) {
						break;
					}
				}
			}
		}
	}
}

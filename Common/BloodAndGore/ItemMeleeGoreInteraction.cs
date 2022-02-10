using Microsoft.Xna.Framework;
using Terraria;
using TerrariaOverhaul.Common.Hooks.Items;
using TerrariaOverhaul.Utilities;
using TerrariaOverhaul.Common.ModEntities.Items.Components;
using TerrariaOverhaul.Common.ModEntities.Items.Components.Melee;

namespace TerrariaOverhaul.Common.BloodAndGore
{
	public sealed class ItemMeleeGoreInteraction : ItemComponent
	{
		public override void HoldItem(Item item, Player player)
		{
			if (!Enabled) {
				return;
			}

			if (player.itemAnimation < player.itemAnimationMax - 1 || !ICanDoMeleeDamage.Hook.Invoke(item, player)) {
				return;
			}

			float range = ItemMeleeAttackAiming.GetAttackRange(item, player);
			float arcRadius = MathHelper.Pi * 0.5f;

			const int MaxHits = 5;

			int numHit = 0;

			if (!item.TryGetGlobalItem(out ItemMeleeAttackAiming meleeAttackAiming)) {
				return;
			}

			for (int i = 0; i < Main.maxGore; i++) {
				if (Main.gore[i] is not OverhaulGore gore || !gore.active || gore.time < 30) {
					continue;
				}

				if (CollisionUtils.CheckRectangleVsArcCollision(gore.AABBRectangle, player.Center, meleeAttackAiming.AttackAngle, arcRadius, range)) {
					gore.HitGore(meleeAttackAiming.AttackDirection);

					if (++numHit >= MaxHits) {
						break;
					}
				}
			}
		}
	}
}

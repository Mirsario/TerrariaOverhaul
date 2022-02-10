using System;
using Microsoft.Xna.Framework;
using Terraria;
using TerrariaOverhaul.Common.ModEntities.NPCs;
using TerrariaOverhaul.Common.Movement;
using TerrariaOverhaul.Utilities.Extensions;

namespace TerrariaOverhaul.Common.ModEntities.Items.Components.Melee
{
	public sealed class ItemMeleeAirCombat : ItemComponent
	{
		public override void ModifyHitNPC(Item item, Player player, NPC target, ref int damage, ref float knockback, ref bool crit)
		{
			if (!Enabled || !item.TryGetGlobalItem<ItemMeleeAttackAiming>(out var meleeAttackAiming)) {
				return;
			}

			// Reduce knockback when the player is in air, and the enemy is somewhat above them.
			if (!player.OnGround() && meleeAttackAiming.AttackDirection.Y < 0.25f) {
				knockback *= 0.75f;
			}
		}

		public override void OnHitNPC(Item item, Player player, NPC target, int damage, float knockback, bool crit)
		{
			if (!Enabled || !item.TryGetGlobalItem<ItemMeleeAttackAiming>(out var meleeAttackAiming)) {
				return;
			}

			var movement = player.GetModPlayer<PlayerMovement>();
			var modifier = PlayerMovement.MovementModifier.Default;

			if (player.velocity.Y != 0f) {
				if (meleeAttackAiming.AttackDirection.Y < 0.1f) {
					modifier.GravityScale *= 0.1f;
				}

				var positionDifference = target.Center - player.Center;
				float distance = positionDifference.SafeLength();
				var dashDirection = target.velocity.SafeNormalize(default);
				var dashVelocity = dashDirection;

				// Boost velocity is based on item knockback.
				float targetSpeed = target.velocity.SafeLength();

				dashVelocity *= Math.Min(Math.Max(2f, targetSpeed), distance / 3f);

				// Reduce intensity when the player is not directly aiming at the enemy.
				float directionsDotProduct = Vector2.Dot(dashDirection, meleeAttackAiming.AttackDirection);

				dashVelocity *= Math.Max(0f, directionsDotProduct * directionsDotProduct);

				// Slight upwards movement bonus.
				dashVelocity.Y -= 1.5f;

				var maxVelocity = Vector2.Min(Vector2.One * 9f, new Vector2(Math.Abs(dashVelocity.X), Math.Abs(dashVelocity.Y)));

				player.AddLimitedVelocity(dashVelocity, maxVelocity);
			}

			movement.SetMovementModifier($"{nameof(ItemMeleeAirCombat)}/{nameof(OnHitNPC)}", player.itemAnimationMax / 2, modifier);
		}
	}
}

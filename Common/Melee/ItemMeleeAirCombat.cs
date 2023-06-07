using System;
using Microsoft.Xna.Framework;
using Terraria;
using TerrariaOverhaul.Common.Movement;
using TerrariaOverhaul.Core.Configuration;
using TerrariaOverhaul.Core.ItemComponents;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.Melee;

public sealed class ItemMeleeAirCombat : ItemComponent
{
	public static readonly ConfigEntry<bool> EnableAirCombat = new(ConfigSide.Both, "Melee", nameof(EnableAirCombat), () => true);

	public Vector2 MinAddedSpeed { get; set; } = Vector2.One * 2.0f;
	public Vector2 MaxAddedSpeed { get; set; } = Vector2.One * 9.0f;
	public Vector2 HardVelocityCap { get; set; } = new Vector2(18.0f, 18.0f);
	public Vector2 FixedVelocityBonus { get; set; } = new Vector2(0.0f, -1.5f);
	public MovementModifier MovementModifier { get; set; } = new() { GravityScale = 0.1f };
	public float MovementModifierLengthMultiplier { get; set; } = 0.5f;

	public override void OnHitNPC(Item item, Player player, NPC target, NPC.HitInfo hit, int damageDone)
	{
		if (!Enabled || !item.TryGetGlobalItem<ItemMeleeAttackAiming>(out var meleeAttackAiming)) {
			return;
		}

		var movement = player.GetModPlayer<PlayerMovement>();
		var keyDirection = player.KeyDirection();

		if (player.velocity.Y != 0f && keyDirection != default) {
			var positionDifference = target.Center - player.Center;
			float distance = positionDifference.SafeLength();

			// Apply movement modifier.

			int modifierLength = (int)MathF.Floor(player.itemAnimationMax * MovementModifierLengthMultiplier);

			movement.SetMovementModifier($"{nameof(ItemMeleeAirCombat)}/{nameof(OnHitNPC)}", modifierLength, MovementModifier);

			var dashDirection = keyDirection.SafeNormalize(-Vector2.UnitY);
			var dashVelocity = dashDirection;

			// Boost velocity is based on enemy's current speed.

			float targetSpeed = target.velocity.SafeLength();
			var targetVelocityDirection = target.velocity.SafeNormalize(-Vector2.UnitY);
			float dashDirectionVsTargetVelocityDirectionDot = (Vector2.Dot(dashDirection, targetVelocityDirection) + 1f) * 0.5f;
			float adjustedTargetSpeed = targetSpeed * dashDirectionVsTargetVelocityDirectionDot;
			Vector2 dashSpeeds = Vector2.Min(Vector2.Max(MinAddedSpeed, Vector2.One * adjustedTargetSpeed), MaxAddedSpeed);

			dashVelocity *= dashSpeeds;

			// Apply fixed velocity bonuses.
			dashVelocity += FixedVelocityBonus;

			// Add the velocity!

			var maxVelocity = Vector2.Min(HardVelocityCap, new Vector2(Math.Abs(dashVelocity.X), Math.Abs(dashVelocity.Y)));

			VelocityUtils.AddLimitedVelocity(player, dashVelocity, maxVelocity);
		}
	}
}

using System;
using Microsoft.Xna.Framework;
using Terraria;
using TerrariaOverhaul.Common.Charging;
using TerrariaOverhaul.Common.Items;
using TerrariaOverhaul.Common.Movement;
using TerrariaOverhaul.Common.PlayerEffects;
using TerrariaOverhaul.Core.ItemComponents;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.Archery;

//TODO: Add more grace-timing to everything.
//TODO: Disallow hovering 1 pixel above the ground.
public sealed class ItemPowerAttackHover : ItemComponent
{
	private static readonly Gradient<float> activationGradient = new(stackalloc Gradient<float>.Key[] {
		(0.00f, 0.0f),
		(0.25f, 1.0f),
		(0.90f, 1.0f),
		(1.00f, 0.0f),
	});

	public bool ControlsVelocityRecoil;
	public Vector4? ActivationVelocityRange = null;
	public MovementModifier Modifier = new() {
		RunAccelerationScale = 0.50f,
		VelocityScale = (Positive: new Vector2(1.00f, 0.10f), Negative: Vector2.One),
	};

	private bool active;
	private bool needsGroundReset;

	public override void HoldItem(Item item, Player player)
	{
		if (!Enabled) {
			return;
		}

		// Player has to be mid-air.
		if (player.velocity.Y == 0f) {
			Stop(item, onGround: true);
			return;
		}

		if (needsGroundReset) {
			return;
		}

		// Player has to be holding the jump button and not be holding down.
		if (!player.controlJump || player.controlDown) {
			Stop(item);
			return;
		}

		// If outside required velocity range when starting - cease.
		if (!active && ActivationVelocityRange is Vector4 range && player.velocity is Vector2 velocity) {
			if (velocity.X < range.X || velocity.X > range.Z || velocity.Y < range.Y || velocity.Y > range.W) {
				Stop(item);
				return;
			}
		}

		// Acquire necessary components.
		if (!item.TryGetGlobalItem(out ItemPowerAttacks powerAttacks) || !player.TryGetModPlayer(out PlayerMovement movement)) {
			return;
		}

		// Must be charging a power attack.
		if (!powerAttacks.Enabled || !powerAttacks.IsCharging) {
			Stop(item);
			return;
		}

		if (!active) {
			Start(item);
		}

		float progressFactor = activationGradient.GetValue(powerAttacks.Charge.Progress);
		float factor = progressFactor;

		var modifier = MovementModifier.Lerp(in MovementModifier.Default, in Modifier, factor);

		if (player.velocity.Y >= -0.5f) {
			modifier.GravityScale *= 0f;
		}

		movement.SetMovementModifier(nameof(ItemPowerAttackHover), 2, modifier);

		if (!Main.dedServ && player.TryGetModPlayer(out PlayerTrailEffects trailEffects)) {
			trailEffects.ForceTrailEffect(3);
		}
	}

	private void Start(Item item)
	{
		if (active) {
			return;
		}

		if (ControlsVelocityRecoil && item.TryGetGlobalItem(out ItemUseVelocityRecoil velocityRecoil)) {
			velocityRecoil.SetEnabled(item, true);
		}

		active = true;
	}

	private void Stop(Item item, bool onGround = false)
	{
		if (onGround) {
			needsGroundReset = false;
		} else if (active) {
			needsGroundReset = true;

			if (ControlsVelocityRecoil && item.TryGetGlobalItem(out ItemUseVelocityRecoil velocityRecoil)) {
				velocityRecoil.SetEnabled(item, false);
			}
		}

		active = false;
	}
}

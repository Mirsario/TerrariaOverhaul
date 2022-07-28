using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using TerrariaOverhaul.Core.Configuration;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.PlayerAnimations;

public sealed class PlayerBodyRotation : ModPlayer
{
	public static readonly ConfigEntry<bool> EnablePlayerTilting = new(ConfigSide.ClientOnly, "PlayerVisuals", nameof(EnablePlayerTilting), () => true);

	public float Rotation;
	public float RotationOffsetScale;

	public override void PreUpdate()
	{
		if (Player.dead) {
			return;
		}

		Player.fullRotationOrigin = new Vector2(11, 22);
	}

	public override void PostUpdate()
	{
		if (Player.sleeping.isSleeping) {
			return;
		}

		if (RotationOffsetScale != 0f && EnablePlayerTilting) {
			float movementRotation;

			if (Player.OnGround()) {
				movementRotation = Player.velocity.X * (Player.velocity.X < Main.MouseWorld.X ? 1f : -1f) * 0.025f;
			} else {
				movementRotation = MathHelper.Clamp(Player.velocity.Y * Math.Sign(Player.velocity.X) * -0.015f, -0.4f, 0.4f);
			}

			Rotation += movementRotation;

			//TODO: If swimming, multiply by 4.
		}

		/*
		while (rotation >= MathHelper.TwoPi) {
			rotation -= MathHelper.TwoPi;
		}
		
		while (rotation <= -MathHelper.TwoPi) {
			rotation += MathHelper.TwoPi;
		}
		*/

		if (!Player.mount.Active) {
			Player.fullRotation = Rotation * Player.gravDir;
		}

		Rotation = 0f;
		RotationOffsetScale = 1f;
	}
}

using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using TerrariaOverhaul.Core.Configuration;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.EntityEffects;

public sealed class PlayerBodyRotation : ModPlayer
{
	public static readonly ConfigEntry<bool> EnablePlayerTilting = new(ConfigSide.ClientOnly, "Visuals", nameof(EnablePlayerTilting), () => true);

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

		// Do nothing for minecarts.
		if (Player.mount is { Active: true, Cart: true }) {
			return;
		}

		if (RotationOffsetScale != 0f && EnablePlayerTilting) {
			float movementRotation = BodyTilting.CalculateRotationOffset(Player.velocity, Player.OnGround(), airMultiplier: 0.8f);

			if (Player.mount.Active) {
				// Reduce intensity on mounts.
				movementRotation *= 0.5f;
			}

			Rotation += movementRotation;

			//TODO: If swimming, multiply by 4.
		}

		Player.fullRotation = Rotation * Player.gravDir;

		Rotation = 0f;
		RotationOffsetScale = 1f;
	}
}

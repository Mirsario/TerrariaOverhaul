using System;
using Microsoft.Xna.Framework;
using Terraria;

namespace TerrariaOverhaul.Common.EntityEffects;

public static class BodyTilting
{
	public static float CalculateRotationOffset(Vector2 movementDelta, bool onGround, float maxTilt = 0.4f, float groundMultiplier = 1f, float airMultiplier = 1f)
	{
		const float BaseMultiplier = 0.025f;

		float movementRotation;

		if (onGround) {
			movementRotation = movementDelta.X * (movementDelta.X < Main.MouseWorld.X ? 1f : -1f) * groundMultiplier;
		} else {
			movementRotation = -movementDelta.Y * Math.Sign(movementDelta.X) * airMultiplier;
		}

		movementRotation *= BaseMultiplier;
		movementRotation = MathHelper.Clamp(movementRotation, -maxTilt, maxTilt);

		return movementRotation;
	}
}

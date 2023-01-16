using System;
using Microsoft.Xna.Framework;
using Terraria;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.Movement;

public static class VelocityUtils
{
	/// <summary>
	/// Returns a vector multiplier that can be used to insert directional input reliance into velocity boosts.
	/// </summary>
	public static Vector2 CalculateDirectionalInputModifierForVelocity(Vector2 velocityDirection, Vector2 moveInput, Vector2 fallbackResults, float zeroedAreaFactor = 0.25f)
	{
		float CalculateForAxis(byte axis, float inputValue, float fallbackResult)
		{
			if (inputValue == 0f) {
				return fallbackResult;
			}

			float otherAxis = 1f - Math.Abs(inputValue);
			Vector2 adjustedKeyDirection = axis == 0
				? new Vector2(inputValue, otherAxis)
				: new Vector2(otherAxis, inputValue);

			adjustedKeyDirection = Vector2.Normalize(adjustedKeyDirection);

			float dotProduct = Vector2.Dot(velocityDirection, adjustedKeyDirection);
			float controlMultiplier = (dotProduct + 1f) * 0.5f;

			controlMultiplier = (controlMultiplier - zeroedAreaFactor) / (1f - zeroedAreaFactor);
			controlMultiplier = MathHelper.Clamp(controlMultiplier, 0f, 1f);

			return controlMultiplier;
		}

		var result = new Vector2(
			CalculateForAxis(0, moveInput.X, fallbackResults.X),
			CalculateForAxis(1, moveInput.Y, fallbackResults.Y)
		);

		return result;
	}

	public static void AddLimitedVelocity(Entity entity, Vector2 addedVelocity, Vector2 maxVelocity)
	{
		if (maxVelocity.X < 0f || maxVelocity.Y < 0f) {
			throw new ArgumentException($"'{nameof(maxVelocity)}' cannot have negative values.");
		}

		if (entity is Player { pulley: true }) { // || oPlayer.OnIce) {
			return;
		}

		if (Math.Sign(entity.velocity.X) != Math.Sign(addedVelocity.X) || Math.Abs(entity.velocity.X) < maxVelocity.X) {
			entity.velocity.X = MathUtils.StepTowards(entity.velocity.X, maxVelocity.X * Math.Sign(addedVelocity.X), Math.Abs(addedVelocity.X));
		}

		if (Math.Sign(entity.velocity.Y) != Math.Sign(addedVelocity.Y) || Math.Abs(entity.velocity.Y) < maxVelocity.Y) {
			entity.velocity.Y = MathUtils.StepTowards(entity.velocity.Y, maxVelocity.Y * Math.Sign(addedVelocity.Y), Math.Abs(addedVelocity.Y));

			// Reset player fall
			if (addedVelocity.Y < 0f && entity is Player player) {
				if (player.velocity.Y < Math.Min(7f, player.maxFallSpeed)) {
					player.fallStart = player.fallStart2 = (int)(player.position.Y / 16f);
				}
			}
		}
	}
}

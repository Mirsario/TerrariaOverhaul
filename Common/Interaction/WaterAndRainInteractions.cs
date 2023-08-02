using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using TerrariaOverhaul.Core.Time;

namespace TerrariaOverhaul.Common.Interaction;

internal static class WaterAndRainInteractions
{
	public static readonly int DebuffId = BuffID.Wet;

	public static int DebuffTime = 10 * TimeSystem.LogicFramerate;
	public static int DebuffTimeThreshold = 5 * TimeSystem.LogicFramerate;

	public static bool LiquidCheck(Entity entity)
	{
		return entity.wet && !entity.honeyWet && !entity.lavaWet;
	}

	public static bool RainCheck(Entity entity)
	{
		if (!Main.raining) {
			return false;
		}

		int maxRain = Main.rain.Length - 1;
		var entityCenter = entity.Center;
		float maxDistance = 1f + MathF.Min(entity.width, entity.height);
		float maxSqrDistance = maxDistance * maxDistance;

		for (int i = 0; i < maxRain; i++) {
			var rain = Main.rain[i];

			if (rain.active && Vector2.DistanceSquared(entityCenter, rain.position) <= maxSqrDistance) {
				return true;
			}
		}

		return false;
	}
}

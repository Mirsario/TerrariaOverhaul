using System;
using Microsoft.Xna.Framework;
using Terraria;

namespace TerrariaOverhaul.Utilities
{
	public static class ItemUtils
	{
		public static float GetHeavyness(Item item)
		{
			const float HeaviestSpeed = 0.5f;
			const float LightestSpeed = 5f;

			float speed = 1f / (Math.Max(1, item.useAnimation) / 60f);
			float speedResult = MathHelper.Clamp(MathUtils.InverseLerp(speed, LightestSpeed, HeaviestSpeed), 0f, 1f);

			float averageDimension = (item.width + item.height) * 0.5f;
			float sizeResult = Math.Max(0f, averageDimension / 10f);

			float result = speedResult * sizeResult;

			return MathHelper.Clamp(result, 0f, 1f);
		}
	}
}

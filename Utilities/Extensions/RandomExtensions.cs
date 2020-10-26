using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Utilities;

namespace TerrariaOverhaul.Utilities.Extensions
{
	public static class RandomExtensions
	{
		public static Vector2 NextVector2(this UnifiedRandom random, float minX, float minY, float maxX, float maxY) => new Vector2(
			random.NextFloat(minX, maxX),
			random.NextFloat(minY, maxY)
		);
	}
}

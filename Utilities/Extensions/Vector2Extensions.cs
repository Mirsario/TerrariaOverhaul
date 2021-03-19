using Microsoft.Xna.Framework;
using Terraria;

namespace TerrariaOverhaul.Utilities.Extensions
{
	public static class Vector2Extensions
	{
		public static bool IsInWorld(this Vector2 vec) => vec.X > 0 && vec.Y > 0 && vec.X < (Main.maxTilesX - 1) * 16f && vec.Y < (Main.maxTilesY - 1) * 16f;

		public static float SafeLength(this Vector2 vec, float defaultValue = 0f)
		{
			float length = vec.Length();

			return float.IsNaN(length) ? defaultValue : length;
		}
	}
}

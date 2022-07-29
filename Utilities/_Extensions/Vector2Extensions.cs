using Microsoft.Xna.Framework;
using Terraria;

namespace TerrariaOverhaul.Utilities;

public static class Vector2Extensions
{
	public static bool IsInWorld(this Vector2 vec)
		=> vec.X > 0 && vec.Y > 0 && vec.X < (Main.maxTilesX - 1) * 16f && vec.Y < (Main.maxTilesY - 1) * 16f;

	public static float SafeLength(this Vector2 vec, float defaultValue = 0f)
	{
		float length = vec.Length();

		return float.IsNaN(length) ? defaultValue : length;
	}

	public static Rectangle ToRectangle(this Vector2 vector, Vector2 size)
		=> vector.ToRectangle((int)size.X, (int)size.Y);

	public static Rectangle ToRectangle(this Vector2 vector, int width, int height)
		=> new((int)vector.X, (int)vector.Y, width, height);
}

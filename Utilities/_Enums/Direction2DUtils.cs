using Microsoft.Xna.Framework;

namespace TerrariaOverhaul.Utilities;

public static class Direction2DUtils
{
	public static sbyte X(this Direction2D direction)
		=> !direction.HasFlag(Direction2D.Left) ? (direction.HasFlag(Direction2D.Right) ? (sbyte)1 : (sbyte)0) : (sbyte)-1;

	public static sbyte Y(this Direction2D direction)
		=> !direction.HasFlag(Direction2D.Up) ? (direction.HasFlag(Direction2D.Down) ? (sbyte)1 : (sbyte)0) : (sbyte)-1;

	public static Vector2 ToVector2(this Direction2D direction)
		=> new(direction.X(), direction.Y());

	public static Direction2D ToDirection2D(this Vector2 vector)
	{
		Direction2D result = 0;

		if (vector.X > 0f) {
			result |= Direction2D.Right;
		} else if (vector.X < 0f) {
			result |= Direction2D.Left;
		}

		if (vector.Y > 0f) {
			result |= Direction2D.Down;
		} else if (vector.Y < 0f) {
			result |= Direction2D.Up;
		}

		return result;
	}
}

// Copyright (c) 2020-2025 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using System;
using Microsoft.Xna.Framework;

namespace TerrariaOverhaul.Utilities.Terraria;

public enum Direction1D : sbyte
{
	Left = -1,
	Right = 1,
}

[Flags]
public enum Direction2D : byte
{
	None = 0,
	Up = 1,
	Down = 2,
	Left = 4,
	Right = 8,
	TopLeft = Up | Left,
	TopRight = Up | Right,
	BottomLeft = Down | Left,
	BottomRight = Down | Right,
}

internal static class DirectionUtils
{
	public static sbyte X(this Direction2D direction)
		=> !direction.HasFlag(Direction2D.Left) ? direction.HasFlag(Direction2D.Right) ? (sbyte)1 : (sbyte)0 : (sbyte)-1;

	public static sbyte Y(this Direction2D direction)
		=> !direction.HasFlag(Direction2D.Up) ? direction.HasFlag(Direction2D.Down) ? (sbyte)1 : (sbyte)0 : (sbyte)-1;

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

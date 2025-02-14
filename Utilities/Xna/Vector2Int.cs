// Copyright (c) 2020-2025 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using System;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;

namespace TerrariaOverhaul.Utilities.Xna;

public struct Vector2Int(int x, int y)
{
	public static readonly int SizeInBytes = Marshal.SizeOf(typeof(Vector2Int));
	public static readonly Vector2Int Zero = default;
	public static readonly Vector2Int One = new(1, 1);
	public static readonly Vector2Int UnitX = new(1, 0);
	public static readonly Vector2Int UnitY = new(0, 1);
	public static readonly Vector2Int Up = new(0, 1);
	public static readonly Vector2Int Down = new(0, -1);
	public static readonly Vector2Int Left = new(-1, 0);
	public static readonly Vector2Int Right = new(1, 0);

	public int X = x;
	public int Y = y;

	public readonly override int GetHashCode() => X ^ Y << 2;
	public readonly override bool Equals(object? other) => other is Vector2Int point && X == point.X && Y == point.Y;
	public readonly override string ToString() => $"X: {X}, Y: {Y}";

	public readonly void Deconstruct(out int x, out int y)
		=> (x, y) = (X, Y);

	public static Vector2Int Max(Vector2Int a, Vector2Int b) => new(Math.Max(a.X, b.X), Math.Max(a.Y, b.Y));
	public static Vector2Int Min(Vector2Int a, Vector2Int b) => new(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y));

	// i32
	public static Vector2Int operator *(Vector2Int a, int d) => new(a.X * d, a.Y * d);
	public static Vector2Int operator *(int d, Vector2Int a) => new(a.X * d, a.Y * d);
	public static Vector2Int operator /(Vector2Int a, int d) => new(a.X / d, a.Y / d);

	// f32
	public static Vector2 operator *(Vector2Int a, float d) => new(a.X * d, a.Y * d);
	public static Vector2 operator *(float d, Vector2Int a) => new(d * a.X, d * a.Y);
	public static Vector2 operator /(Vector2Int a, float d) => new(a.X / d, a.Y / d);

	// Vector2Int
	public static Vector2Int operator +(Vector2Int a, Vector2Int b) => new(a.X + b.X, a.Y + b.Y);
	public static Vector2Int operator -(Vector2Int a, Vector2Int b) => new(a.X - b.X, a.Y - b.Y);
	public static Vector2Int operator *(Vector2Int a, Vector2Int b) => new(a.X * b.X, a.Y * b.Y);
	public static Vector2Int operator /(Vector2Int a, Vector2Int b) => new(a.X / b.X, a.Y / b.Y);
	public static Vector2Int operator -(Vector2Int a) => new(-a.X, -a.Y);
	public static bool operator ==(Vector2Int a, Vector2Int b) => a.X == b.X && a.Y == b.Y;
	public static bool operator !=(Vector2Int a, Vector2Int b) => a.X != b.X || a.Y != b.Y;

	// Vector2
	public static Vector2 operator +(Vector2 a, Vector2Int b) => new(a.X + b.X, a.Y + b.Y);
	public static Vector2 operator -(Vector2 a, Vector2Int b) => new(a.X - b.X, a.Y - b.Y);
	public static Vector2 operator *(Vector2 a, Vector2Int b) => new(a.X * b.X, a.Y * b.Y);
	public static Vector2 operator /(Vector2 a, Vector2Int b) => new(a.X / b.X, a.Y / b.Y);
	public static Vector2 operator +(Vector2Int a, Vector2 b) => new(a.X + b.X, a.Y + b.Y);
	public static Vector2 operator -(Vector2Int a, Vector2 b) => new(a.X - b.X, a.Y - b.Y);
	public static Vector2 operator *(Vector2Int a, Vector2 b) => new(a.X * b.X, a.Y * b.Y);
	public static Vector2 operator /(Vector2Int a, Vector2 b) => new(a.X / b.X, a.Y / b.Y);
	public static bool operator ==(Vector2Int a, Vector2 b) => a.X == b.X && a.Y == b.Y;
	public static bool operator ==(Vector2 a, Vector2Int b) => a.X == b.X && a.Y == b.Y;
	public static bool operator !=(Vector2Int a, Vector2 b) => a.X != b.X || a.Y != b.Y;
	public static bool operator !=(Vector2 a, Vector2Int b) => a.X != b.X || a.Y != b.Y;

	// Casts
	public static implicit operator Vector2(Vector2Int value) => new(value.X, value.Y);
	public static explicit operator Vector2Int(Vector2 value) => new((int)value.X, (int)value.Y);
	public static implicit operator Point(Vector2Int value) => new(value.X, value.Y);
	public static implicit operator Vector2Int(Point value) => new(value.X, value.Y);
}

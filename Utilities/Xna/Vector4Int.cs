// Copyright (c) 2020-2025 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;

namespace TerrariaOverhaul.Utilities.Xna;

public struct Vector4Int(int x, int y, int z, int w)
{
	public static readonly int SizeInBytes = Marshal.SizeOf(typeof(Vector4Int));
	public static readonly Vector4Int Zero = default;
	public static readonly Vector4Int One = new(1, 1, 1, 1);
	public static readonly Vector4Int UnitX = new(1, 0, 0, 0);
	public static readonly Vector4Int UnitY = new(0, 1, 0, 0);
	public static readonly Vector4Int UnitZ = new(0, 0, 1, 0);
	public static readonly Vector4Int UnitW = new(0, 0, 0, 1);

	public int X = x;
	public int Y = y;
	public int Z = z;
	public int W = w;

	public readonly override string ToString() => $"X: {X}, Y: {Y}, Z: {Z}, W: {W}";
	public readonly override int GetHashCode() => X.GetHashCode() ^ Y.GetHashCode() << 2 ^ Z.GetHashCode() >> 2 ^ W.GetHashCode() >> 1;
	public readonly override bool Equals(object? other) => other is Vector4Int v && X == v.X && Y == v.Y && Z == v.Z && W == v.W;

	public readonly void Deconstruct(out int x, out int y, out int z, out int w)
		=> (x, y, z, w) = (X, Y, Z, W);

	// int
	public static Vector4Int operator *(Vector4Int a, int d) => new(a.X * d, a.Y * d, a.Z * d, a.W * d);
	public static Vector4Int operator *(int d, Vector4Int a) => new(a.X * d, a.Y * d, a.Z * d, a.W * d);
	public static Vector4Int operator /(Vector4Int a, int d) => new(a.X / d, a.Y / d, a.Z / d, a.W / d);

	// float
	public static Vector4 operator *(Vector4Int a, float d) => new(a.X * d, a.Y * d, a.Z * d, a.W * d);
	public static Vector4 operator *(float d, Vector4Int a) => new(d * a.X, d * a.Y, d * a.Z, d * a.W);
	public static Vector4 operator /(Vector4Int a, float d) => new(a.X / d, a.Y / d, a.Z / d, a.W / d);

	// Vector4Int
	public static Vector4Int operator +(Vector4Int a, Vector4Int b) => new(a.X + b.X, a.Y + b.Y, a.Z + b.Z, a.W + b.W);
	public static Vector4Int operator -(Vector4Int a, Vector4Int b) => new(a.X - b.X, a.Y - b.Y, a.Z - b.Z, a.W - b.W);
	public static Vector4Int operator *(Vector4Int a, Vector4Int b) => new(a.X * b.X, a.Y * b.Y, a.Z * b.Z, a.W * b.W);
	public static Vector4Int operator /(Vector4Int a, Vector4Int b) => new(a.X / b.X, a.Y / b.Y, a.Z / b.Z, a.W / b.W);
	public static Vector4Int operator -(Vector4Int a) => new(-a.X, -a.Y, -a.Z, -a.W);
	public static bool operator ==(Vector4Int a, Vector4Int b) => a.X == b.X && a.Y == b.Y && a.Z == b.Z && a.W == b.W;
	public static bool operator !=(Vector4Int a, Vector4Int b) => a.X != b.X || a.Y != b.Y || a.Z != b.Z || a.W != b.W;
}

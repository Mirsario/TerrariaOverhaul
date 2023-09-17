using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;

namespace TerrariaOverhaul.Utilities;

public struct Vector4Int
{
	public static readonly int SizeInBytes = Marshal.SizeOf(typeof(Vector4Int));
	public static readonly Vector4Int Zero = default;
	public static readonly Vector4Int One = new(1, 1, 1, 1);
	public static readonly Vector4Int UnitX = new(1, 0, 0, 0);
	public static readonly Vector4Int UnitY = new(0, 1, 0, 0);
	public static readonly Vector4Int UnitZ = new(0, 0, 1, 0);
	public static readonly Vector4Int UnitW = new(0, 0, 0, 1);

	public int X;
	public int Y;
	public int Z;
	public int W;

	public Vector4Int(int x, int y, int z, int w)
	{
		X = x;
		Y = y;
		Z = z;
		W = w;
	}

	public override string ToString()
		=> $"X: {X}, Y: {Y}, Z: {Z}, W: {W}";
	
	public override int GetHashCode()
		=> X.GetHashCode() ^ Y.GetHashCode() << 2 ^ Z.GetHashCode() >> 2 ^ W.GetHashCode() >> 1;
	
	public override bool Equals(object? other)
		=> other is Vector4Int v && X == v.X && Y == v.Y && Z == v.Z && W == v.W;

	public readonly void Deconstruct(out int x, out int y)
	{
		x = X;
		y = Y;
	}

	// Vector4Int

	public static Vector4Int operator +(Vector4Int a, Vector4Int b) => new(a.X + b.X, a.Y + b.Y, a.Z + b.Z, a.W + b.W);
	
	public static Vector4Int operator -(Vector4Int a, Vector4Int b) => new(a.X - b.X, a.Y - b.Y, a.Z - b.Z, a.W - b.W);
	
	public static Vector4Int operator *(Vector4Int a, Vector4Int b) => new(a.X * b.X, a.Y * b.Y, a.Z * b.Z, a.W * b.W);
	
	public static Vector4Int operator /(Vector4Int a, Vector4Int b) => new(a.X / b.X, a.Y / b.Y, a.Z / b.Z, a.W / b.W);
	
	public static Vector4Int operator -(Vector4Int a) => new(-a.X, -a.Y, -a.Z, -a.W);
	
	public static bool operator ==(Vector4Int a, Vector4Int b) => a.X == b.X && a.Y == b.Y && a.Z == b.Z && a.W == b.W;
	
	public static bool operator !=(Vector4Int a, Vector4Int b) => a.X != b.X || a.Y != b.Y || a.Z != b.Z || a.W != b.W;
	
	// int
	
	public static Vector4Int operator *(Vector4Int a, int d) => new(a.X * d, a.Y * d, a.Z * d, a.W * d);
	
	public static Vector4Int operator *(int d, Vector4Int a) => new(a.X * d, a.Y * d, a.Z * d, a.W * d);
	
	public static Vector4Int operator /(Vector4Int a, int d) => new(a.X / d, a.Y / d, a.Z / d, a.W / d);
	
	// float
	
	public static Vector4 operator *(Vector4Int a, float d) => new(a.X * d, a.Y * d, a.Z * d, a.W * d);
	
	public static Vector4 operator *(float d, Vector4Int a) => new(d * a.X, d * a.Y, d * a.Z, d * a.W);
	
	public static Vector4 operator /(Vector4Int a, float d) => new(a.X / d, a.Y / d, a.Z / d, a.W / d);
}

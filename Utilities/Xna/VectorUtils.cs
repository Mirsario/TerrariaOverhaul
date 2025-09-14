// Copyright (c) 2020-2025 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using System;
using System.IO;
using Microsoft.Xna.Framework;

namespace TerrariaOverhaul.Utilities.Xna;

internal static class VectorUtils
{
	public static Vector2 StepTowards(Vector2 value, Vector2 goal, float step)
		=> StepTowards(value, goal, new Vector2(step, step));

	public static Vector2 StepTowards(Vector2 value, Vector2 goal, Vector2 step) => new(
		MathUtils.StepTowards(value.X, goal.X, step.X),
		MathUtils.StepTowards(value.Y, goal.Y, step.Y)
	);

	public static Vector2 Round(Vector2 value) => new(MathF.Round(value.X), MathF.Round(value.Y));
	public static Vector3 Round(Vector3 value) => new(MathF.Round(value.X), MathF.Round(value.Y), MathF.Round(value.Z));
	public static Vector4 Round(Vector4 value) => new(MathF.Round(value.X), MathF.Round(value.Y), MathF.Round(value.Z), MathF.Round(value.W));

	public static Vector2 Floor(Vector2 value) => new(MathF.Floor(value.X), MathF.Floor(value.Y));
	public static Vector3 Floor(Vector3 value) => new(MathF.Floor(value.X), MathF.Floor(value.Y), MathF.Floor(value.Z));
	public static Vector4 Floor(Vector4 value) => new(MathF.Floor(value.X), MathF.Floor(value.Y), MathF.Floor(value.Z), MathF.Floor(value.W));

	public static Vector2 Ceil(Vector2 value) => new(MathF.Ceiling(value.X), MathF.Ceiling(value.Y));
	public static Vector3 Ceil(Vector3 value) => new(MathF.Ceiling(value.X), MathF.Ceiling(value.Y), MathF.Ceiling(value.Z));
	public static Vector4 Ceil(Vector4 value) => new(MathF.Ceiling(value.X), MathF.Ceiling(value.Y), MathF.Ceiling(value.Z), MathF.Ceiling(value.W));

	public static void Deconstruct(this Vector2 vec, out float x, out float y)
		=> (x, y) = (vec.X, vec.Y);
	public static void Deconstruct(this Vector3 vec, out float x, out float y, out float z)
		=> (x, y, z) = (vec.X, vec.Y, vec.Z);
	public static void Deconstruct(this Vector4 vec, out float x, out float y, out float z, out float w)
		=> (x, y, z, w) = (vec.X, vec.Y, vec.Z, vec.W);

	public static float SafeLength(this Vector2 vec, float defaultValue = 0f)
	{
		float length = vec.Length();

		return float.IsNaN(length) ? defaultValue : length;
	}

	public static Rectangle ToRectangle(this Vector2 vector, Vector2 size) => vector.ToRectangle((int)size.X, (int)size.Y);
	public static Rectangle ToRectangle(this Vector2 vector, int width, int height) => new((int)vector.X, (int)vector.Y, width, height);

	// IO

	public static Vector2 ReadHalfVector2(this BinaryReader reader)
		=> new((float)reader.ReadHalf(), (float)reader.ReadHalf());
	public static Vector2Int ReadVector2Int(this BinaryReader reader)
		=> new(reader.ReadInt32(), reader.ReadInt32());

	public static void WriteVector2Int(this BinaryWriter writer, Vector2Int vector)
	{
		writer.Write(vector.X);
		writer.Write(vector.Y);
	}
	public static void WriteVector2F16(this BinaryWriter writer, Vector2 vector)
	{
		writer.Write((Half)vector.X);
		writer.Write((Half)vector.Y);
	}
}

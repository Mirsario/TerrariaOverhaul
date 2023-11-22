using System;
using Microsoft.Xna.Framework;
using Terraria;

namespace TerrariaOverhaul.Utilities;

public static class RectangleExtensions
{
	// Contains

	public static bool Contains(this Rectangle rect, Vector2 point)
	{
		return rect.X >= point.X
			&& rect.Y >= point.Y
			&& rect.Right <= point.X
			&& rect.Bottom <= point.Y;
	}

	public static bool ContainsWithThreshold(this Rectangle rect, Vector2 point, float threshold)
		=> ContainsWithThreshold(rect, point, new Vector2(threshold, threshold));

	public static bool ContainsWithThreshold(this Rectangle rect, Vector2 point, Vector2 threshold)
	{
		return rect.X - threshold.X >= point.X
			&& rect.Y - threshold.Y >= point.Y
			&& rect.Right + threshold.X <= point.X
			&& rect.Bottom + threshold.Y <= point.Y;
	}

	// Resizing

	public static Rectangle Extended(this Rectangle rect, int extents)
	{
		return new Rectangle(rect.X - extents, rect.Y - extents, rect.Width + extents + extents, rect.Height + extents + extents);
	}

	public static Rectangle Extended(this Rectangle rect, Vector2Int extents)
	{
		return new Rectangle(rect.X - extents.X, rect.Y - extents.Y, rect.Width + extents.X + extents.X, rect.Height + extents.Y + extents.Y);
	}

	public static Rectangle Extended(this Rectangle rect, Vector4Int extents)
	{
		return new Rectangle(rect.X - extents.X, rect.Y - extents.Y, rect.Width + extents.X + extents.Z, rect.Height + extents.Y + extents.W);
	}

	// World / Tile conversions

	public static Rectangle ToTileCoordinates(this Rectangle rect)
	{
		return new Rectangle(rect.X / 16, rect.Y / 16, rect.Width / 16, rect.Height / 16);
	}

	public static Rectangle ToWorldCoordinates(this Rectangle rect)
	{
		return new Rectangle(rect.X * 16, rect.Y * 16, rect.Width * 16, rect.Height * 16);
	}

	// Clamping

	public static Rectangle ClampTileCoordinates(this Rectangle rect)
	{
		var points = new Vector4Int(
			Math.Min(Math.Max(rect.Left, 0), Main.maxTilesX),
			Math.Min(Math.Max(rect.Top, 0), Main.maxTilesY),
			Math.Min(Math.Max(rect.Right, 0), Main.maxTilesX),
			Math.Min(Math.Max(rect.Bottom, 0), Main.maxTilesY)
		);

		return new Rectangle(points.X, points.Y, points.Z - points.X, points.W - points.Y);
	}

	public static Rectangle ClampWorldCoordinates(this Rectangle rect)
	{
		var points = new Vector4Int(
			Math.Min(Math.Max(rect.Left, 0), (int)Main.rightWorld),
			Math.Min(Math.Max(rect.Top, 0), (int)Main.leftWorld),
			Math.Min(Math.Max(rect.Right, 0), (int)Main.rightWorld),
			Math.Min(Math.Max(rect.Bottom, 0), (int)Main.leftWorld)
		);

		return new Rectangle(points.X, points.Y, points.Z - points.X, points.W - points.Y);
	}

	// Random

	public static Vector2 GetRandomPoint(this Rectangle rect)
	{
		return new Vector2(
			rect.X + Main.rand.NextFloat(rect.Width),
			rect.Y + Main.rand.NextFloat(rect.Height)
		);
	}

	// Etc

	public static Vector2 GetCorner(this Rectangle rect, Vector2 point)
	{
		var topLeft = rect.TopLeft();
		var bottomRight = rect.BottomRight();
		var result = point;

		result.X = MathHelper.Clamp(point.X, topLeft.X, bottomRight.X);
		result.Y = MathHelper.Clamp(point.Y, topLeft.Y, bottomRight.Y);

		return result;
	}
}

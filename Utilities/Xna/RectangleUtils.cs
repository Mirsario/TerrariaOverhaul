// Copyright (c) 2020-2025 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using Microsoft.Xna.Framework;

namespace TerrariaOverhaul.Utilities.Xna;

public static class RectangleUtils
{
	// Contains

	public static bool Contains(this Rectangle rect, Vector2 point)
		=> rect.X >= point.X
		&& rect.Y >= point.Y
		&& rect.Right <= point.X
		&& rect.Bottom <= point.Y;

	public static bool ContainsWithThreshold(this Rectangle rect, Vector2 point, float threshold)
		=> rect.ContainsWithThreshold(point, new Vector2(threshold, threshold));

	public static bool ContainsWithThreshold(this Rectangle rect, Vector2 point, Vector2 threshold)
		=> rect.X - threshold.X >= point.X
		&& rect.Y - threshold.Y >= point.Y
		&& rect.Right + threshold.X <= point.X
		&& rect.Bottom + threshold.Y <= point.Y;

	// Resizing

	public static Rectangle Extended(this Rectangle rect, int extents)
		=> new(rect.X - extents, rect.Y - extents, rect.Width + extents + extents, rect.Height + extents + extents);
	public static Rectangle Extended(this Rectangle rect, Vector2Int extents)
		=> new(rect.X - extents.X, rect.Y - extents.Y, rect.Width + extents.X + extents.X, rect.Height + extents.Y + extents.Y);
	public static Rectangle Extended(this Rectangle rect, Vector4Int extents)
		=> new(rect.X - extents.X, rect.Y - extents.Y, rect.Width + extents.X + extents.Z, rect.Height + extents.Y + extents.W);

	// Corners

	public static Vector2 GetCorner(this RectFloat rect, Vector2 point) => new(
		MathHelper.Clamp(point.X, rect.X, rect.Right),
		MathHelper.Clamp(point.Y, rect.Y, rect.Bottom)
	);
	public static Vector2 GetCorner(this Rectangle rect, Vector2 point) => new(
		MathHelper.Clamp(point.X, rect.X, rect.X + rect.Width),
		MathHelper.Clamp(point.Y, rect.Y, rect.Y + rect.Height)
	);
}

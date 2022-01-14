using Microsoft.Xna.Framework;
using Terraria;

namespace TerrariaOverhaul.Utilities.Extensions
{
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

		public static bool ContainsWithThreshold(this Rectangle rect, Vector2 point, float threshold) => ContainsWithThreshold(rect, point, new Vector2(threshold, threshold));
		public static bool ContainsWithThreshold(this Rectangle rect, Vector2 point, Vector2 threshold)
		{
			return rect.X - threshold.X >= point.X
				&& rect.Y - threshold.Y >= point.Y
				&& rect.Right + threshold.X <= point.X
				&& rect.Bottom + threshold.Y <= point.Y;
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
}

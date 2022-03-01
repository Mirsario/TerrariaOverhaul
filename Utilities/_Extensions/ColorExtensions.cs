using System;
using Microsoft.Xna.Framework;

namespace TerrariaOverhaul.Utilities
{
	internal static class ColorExtensions
	{
		public static Color WithAlpha(this Color color, byte alpha)
		{
			color.A = alpha;

			return color;
		}

		public static Color WithAlpha(this Color color, float alpha)
		{
			color.A = (byte)(MathHelper.Clamp(alpha, 0f, 1f) * 255f);

			return color;
		}

		public static float GetHue(this Color color)
		{
			if (color.R == color.G && color.G == color.B) {
				return 0f;
			}

			float r = color.R / 255.0f;
			float g = color.G / 255.0f;
			float b = color.B / 255.0f;

			float max, min;
			float delta;
			float hue = 0.0f;

			max = Math.Max(r, Math.Max(g, b));
			min = Math.Min(r, Math.Min(g, b));

			delta = max - min;

			if (r == max) {
				hue = (g - b) / delta;
			} else if (g == max) {
				hue = 2 + (b - r) / delta;
			} else if (b == max) {
				hue = 4 + (r - g) / delta;
			}

			hue *= 60;

			if (hue < 0.0f) {
				hue += 360.0f;
			}

			return hue;
		}

		public static string ToHexRGB(this Color color) => BitConverter.ToString(new byte[] { color.R, color.G, color.B }).Replace("-", "");
		public static string ToHexRGBA(this Color color) => BitConverter.ToString(new byte[] { color.R, color.G, color.B, color.A }).Replace("-", "");
		public static string ToHexARGB(this Color color) => BitConverter.ToString(new byte[] { color.A, color.R, color.G, color.B }).Replace("-", "");
	}
}

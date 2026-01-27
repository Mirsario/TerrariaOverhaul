// Copyright (c) 2020-2026 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using System;
using Microsoft.Xna.Framework;

namespace TerrariaOverhaul.Utilities.Xna;

internal static class ColorUtils
{
	public static Color FromHexRgb(uint hexRgba)
	{
		return new Color(
			(byte)(hexRgba >> 16),
			(byte)(hexRgba >> 8),
			(byte)(hexRgba >> 0),
			255
		);
	}

	public static Color FromHexRgba(uint hexRgba)
	{
		return new Color(
			(byte)(hexRgba >> 24),
			(byte)(hexRgba >> 16),
			(byte)(hexRgba >> 8),
			(byte)(hexRgba >> 0)
		);
	}

	public static string ToHexRGB(this Color color) => BitConverter.ToString(new byte[] { color.R, color.G, color.B }).Replace("-", "");
	public static string ToHexRGBA(this Color color) => BitConverter.ToString(new byte[] { color.R, color.G, color.B, color.A }).Replace("-", "");
	public static string ToHexARGB(this Color color) => BitConverter.ToString(new byte[] { color.A, color.R, color.G, color.B }).Replace("-", "");

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
}

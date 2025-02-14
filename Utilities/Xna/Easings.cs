using System;

namespace TerrariaOverhaul.Utilities.Xna;

internal static class Easings
{
	public static float FadeIn(float x, float start, float end)
		=> MathUtils.Clamp01((x - start) / (end - start));

	public static float FadeOut(float x, float start, float end)
		=> MathUtils.Clamp01(1f - (x - start) / (end - start));

	public static float EaseInSquare(float x) => x * x;
	public static float EaseInCubic(float x) => x * x * x;
	public static float EaseOutSquare(float x) => 1f - MathF.Pow(1f - x, 2f);
	public static float EaseOutCubic(float x) => 1f - MathF.Pow(1f - x, 3f);
}

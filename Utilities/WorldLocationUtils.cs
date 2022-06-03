using Terraria;

namespace TerrariaOverhaul.Utilities
{
	public static class WorldLocationUtils
	{
		public static Gradient<float> SurfaceOrSkyGradient => new(
			(0f, 0f),
			((float)Main.worldSurface * 0.2f, 1f),
			((float)Main.worldSurface, 1f),
			((float)Main.worldSurface * 1.05f, 0f)
		);

		public static Gradient<float> SurfaceGradient => new(
			((float)Main.worldSurface * 0.45f, 0f),
			((float)Main.worldSurface, 1f),
			((float)Main.worldSurface * 1.05f, 0f)
		);

		public static Gradient<float> UnderSurfaceGradient => new(
			((float)Main.worldSurface, 0f),
			((float)Main.worldSurface * 1.25f, 1f)
		);

		public static Gradient<float> SpaceGradient => new(
			((float)Main.worldSurface * 0.15f, 1f),
			((float)Main.worldSurface * 0.4f, 0f)
		);
	}
}

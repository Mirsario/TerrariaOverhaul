using Terraria;
using TerrariaOverhaul.Utilities.DataStructures;

namespace TerrariaOverhaul.Utilities
{
	public static class WorldLocationUtils
	{
		public static Gradient<float> SurfaceOrSkyGradient => new Gradient<float>(
			(0f, 0f),
			((float)Main.worldSurface * 0.2f, 1f),
			((float)Main.worldSurface, 1f),
			((float)Main.worldSurface * 1.05f, 0f)
		);

		public static Gradient<float> SurfaceGradient => new Gradient<float>(
			((float)Main.worldSurface * 0.45f, 0f),
			((float)Main.worldSurface, 1f),
			((float)Main.worldSurface * 1.05f, 0f)
		);

		public static Gradient<float> UnderSurfaceGradient => new Gradient<float>(
			((float)Main.worldSurface, 0f),
			((float)Main.worldSurface * 1.25f, 1f)
		);

		public static Gradient<float> SpaceGradient => new Gradient<float>(
			((float)Main.worldSurface * 0.15f, 1f),
			((float)Main.worldSurface * 0.4f, 0f)
		);
	}
}

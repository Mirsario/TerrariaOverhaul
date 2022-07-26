using Terraria;

namespace TerrariaOverhaul.Utilities;

public static class WorldLocationUtils
{
	public static Gradient<float> SurfaceOrSkyGradient => new(
		(0f, 0f),
		((float)Main.worldSurface * 0.22f, 1f),
		((float)Main.worldSurface * 1.00f, 1f),
		((float)Main.worldSurface * 1.05f, 0f)
	);

	public static Gradient<float> SurfaceGradient => new(
		((float)Main.worldSurface * 0.22f, 0f),
		((float)Main.worldSurface * 0.60f, 1f),
		((float)Main.worldSurface * 1.00f, 1f),
		((float)Main.worldSurface * 1.02f, 1f),
		((float)Main.worldSurface * 1.10f, 0f)
	);

	public static Gradient<float> UnderSurfaceGradient => new(
		((float)Main.worldSurface * 1.02f, 0f),
		((float)Main.worldSurface * 1.10f, 1f),
		((float)Main.worldSurface * 1.25f, 1f)
	);

	public static Gradient<float> SpaceGradient => new(
		(0f, 1f),
		((float)Main.worldSurface * 0.15f, 1f),
		((float)Main.worldSurface * 0.22f, 0f)
	);
}

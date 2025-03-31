// Copyright (c) 2020-2025 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using Terraria;
using TerrariaOverhaul.Utilities.Xna;

namespace TerrariaOverhaul.Utilities.Terraria;

public static class WorldLayers
{
	public static Gradient<float> SurfaceOrSky => new(
		(0f, 0f),
		((float)Main.worldSurface * 0.22f, 1f),
		((float)Main.worldSurface * 1.00f, 1f),
		((float)Main.worldSurface * 1.05f, 0f)
	);

	public static Gradient<float> Surface => new(
		((float)Main.worldSurface * 0.22f, 0f),
		((float)Main.worldSurface * 0.60f, 1f),
		((float)Main.worldSurface * 1.00f, 1f),
		((float)Main.worldSurface * 1.02f, 1f),
		((float)Main.worldSurface * 1.10f, 0f)
	);

	public static Gradient<float> UnderSurface => new(
		((float)Main.worldSurface * 1.02f, 0f),
		((float)Main.worldSurface * 1.10f, 1f),
		((float)Main.worldSurface * 1.25f, 1f)
	);

	public static Gradient<float> Space => new(
		(0f, 1f),
		((float)Main.worldSurface * 0.15f, 1f),
		((float)Main.worldSurface * 0.22f, 0f)
	);
}

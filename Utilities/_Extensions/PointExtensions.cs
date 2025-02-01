// Copyright (c) 2020-2025 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using Microsoft.Xna.Framework;
using Terraria;

namespace TerrariaOverhaul.Utilities;

public static class PointExtensions
{
	public static bool IsInWorld(this Point point) => point.X >= 0 && point.Y >= 0 && point.X < Main.maxTilesX && point.Y < Main.maxTilesY;
}

// Copyright (c) 2020-2025 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using Terraria;
using Terraria.DataStructures;

namespace TerrariaOverhaul.Utilities;

public static class Point16Extensions
{
	public static bool IsInWorld(this Point16 point) => point.X >= 0 && point.Y >= 0 && point.X < Main.maxTilesX && point.Y < Main.maxTilesY;
}

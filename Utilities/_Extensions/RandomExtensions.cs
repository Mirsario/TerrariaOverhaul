// Copyright (c) 2020-2025 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Utilities;

namespace TerrariaOverhaul.Utilities;

public static class RandomExtensions
{
	public static Vector2 NextVector2(this UnifiedRandom random, float minX, float minY, float maxX, float maxY) => new(
		random.NextFloat(minX, maxX),
		random.NextFloat(minY, maxY)
	);
}

// Copyright (c) 2020-2025 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using Microsoft.Xna.Framework;
using ReLogic.Utilities;

namespace TerrariaOverhaul.Utilities.Terraria;

public static class Vector2DUtils
{
	public static Vector2 ToF32(this Vector2D vec) => new((float)vec.X, (float)vec.Y);
	public static Vector2D ToF64(this Vector2 vec) => new(vec.X, vec.Y);
}

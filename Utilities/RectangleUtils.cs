// Copyright (c) 2020-2025 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using Microsoft.Xna.Framework;

namespace TerrariaOverhaul.Utilities;

public static class RectangleUtils
{
	public static Rectangle FromPoints(int left, int up, int right, int bottom)
		=> new(left, up, right - left, bottom - up);
}

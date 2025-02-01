// Copyright (c) 2020-2025 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using Microsoft.Xna.Framework;

namespace TerrariaOverhaul.Utilities;

public static class PlayerFramesExtensions
{
	private const int PlayerSheetWidth = 40;
	private const int PlayerSheetHeight = 56;

	public static Rectangle ToRectangle(this PlayerFrames frame)
	{
		return new Rectangle(0, (int)frame * PlayerSheetHeight, PlayerSheetWidth, PlayerSheetHeight);
	}
}

// Copyright (c) 2020-2026 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using Microsoft.Xna.Framework;

namespace TerrariaOverhaul.Utilities.Terraria;

public enum PlayerFrames
{
	Idle,
	Use1,
	Use2,
	Use3,
	Use4,
	Jump,
	Walk1,
	Walk2,
	Walk3,
	Walk4,
	Walk5,
	Walk6,
	Walk7,
	Walk8,
	Walk9,
	Walk10,
	Walk11,
	Walk12,
	Walk13,
	Walk14,
	Count,
}

internal static class PlayerFramesExtensions
{
	private const int PlayerSheetWidth = 40;
	private const int PlayerSheetHeight = 56;

	public static Rectangle ToRectangle(this PlayerFrames frame)
	{
		return new Rectangle(0, (int)frame * PlayerSheetHeight, PlayerSheetWidth, PlayerSheetHeight);
	}
}

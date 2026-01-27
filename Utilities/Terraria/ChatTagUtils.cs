// Copyright (c) 2020-2026 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using Microsoft.Xna.Framework;
using TerrariaOverhaul.Utilities.Xna;

namespace TerrariaOverhaul.Utilities.Terraria;

internal static class ChatTagUtils
{
	public static string ColoredText(Color color, string text)
		=> $"[c/{color.ToHexRGB()}:{text}]";
}

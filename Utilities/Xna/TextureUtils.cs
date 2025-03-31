// Copyright (c) 2020-2025 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TerrariaOverhaul.Utilities.Xna;

internal static class TextureUtils
{
	public static void InitializeWithColor(Texture2D texture, Color color)
	{
		var data = new Color[texture.Width * texture.Height];
		Array.Fill(data, color);
		texture.SetData(data);
	}
}

// Copyright (c) 2020-2025 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.ModLoader;

namespace TerrariaOverhaul.Utilities;

public static class TextureUtils
{
	private static Asset<Texture2D>? placeholderTexture;

	public static Asset<Texture2D> GetPlaceholderTexture()
		=> placeholderTexture ??= ModContent.Request<Texture2D>($"{nameof(TerrariaOverhaul)}/Assets/Textures/NoTexture");
}

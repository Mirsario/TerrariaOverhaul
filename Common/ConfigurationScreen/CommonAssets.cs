// Copyright (c) 2020-2025 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.ModLoader;
using TerrariaOverhaul.Utilities.Terraria;
using TerrariaOverhaul.Utilities.Xna;

namespace TerrariaOverhaul.Common.ConfigurationScreen;

internal static class CommonAssets
{
	private const string BasePath = $"{nameof(TerrariaOverhaul)}/Assets/Textures/UI/Config";

	private static Asset<Texture2D>? unselectedIconBorderTexture;
	private static Asset<Texture2D>? unknownOptionTexture;
	private static Asset<Texture2D>[]? backgroundTextures;

	public static Asset<Texture2D> UnselectedIconBorderTexture
		=> unselectedIconBorderTexture ??= ModContent.Request<Texture2D>($"{BasePath}/UnselectedIconBorder").EnsureLoaded();

	public static Asset<Texture2D> UnknownOptionTexture
		=> unknownOptionTexture ??= ModContent.Request<Texture2D>($"{BasePath}/UnknownOption").EnsureLoaded();

	public static Asset<Texture2D>[] BackgroundTextures
		=> backgroundTextures ??= Enumerable.Range(1, 7).Select(i => ModContent.Request<Texture2D>($"{BasePath}/Background{i}").EnsureLoaded()).ToArray();

	public static Asset<Texture2D> GetBackgroundTexture(int wrappedIndex)
	{
		return BackgroundTextures[MathUtils.Modulo(wrappedIndex, BackgroundTextures.Length)];
	}
}

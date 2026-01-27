// Copyright (c) 2020-2026 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Drawing;
using Terraria.ModLoader;
using TerrariaOverhaul.Utilities;
using TerrariaOverhaul.Utilities.Terraria;
using TerrariaOverhaul.Utilities.Xna;

namespace TerrariaOverhaul.Core.Tiles;

[Autoload(Side = ModSide.Client)]
internal sealed class TileSnapshotSystem : ModSystem
{
	private static SpriteBatch? spriteBatch;
	private static TileDrawing? tileRenderer;

	public override void Load()
	{

	}

	public override void Unload()
	{
		spriteBatch?.Dispose();
		spriteBatch = null;
	}

	private static void EnsureInitialized()
	{
		tileRenderer ??= new TileDrawing(Main.instance.TilePaintSystem);
	}

	public static RenderTarget2D CreateSpecificTilesSnapshot(Vector2Int sizeInTiles, Vector2Int baseTilePosition, ReadOnlySpan<Vector2Int> tilePositions)
	{
		if (!Program.IsMainThread) {
			throw new InvalidOperationException($"{nameof(CreateSpecificTilesSnapshot)} can only be called on the main thread.");
		}

		var graphicsDevice = Main.graphics.GraphicsDevice;
		var originalRenderTargets = graphicsDevice.GetRenderTargets();

		var textureSize = sizeInTiles * Vector2Int.One * WorldUtils.TileSizeInPixels;
		var renderTarget = new RenderTarget2D(graphicsDevice, textureSize.X, textureSize.Y, false, SurfaceFormat.Color, DepthFormat.None);

		graphicsDevice.SetRenderTarget(renderTarget);
		graphicsDevice.Clear(Color.Transparent);

		RenderSpecificTiles(baseTilePosition, tilePositions);

		graphicsDevice.SetRenderTargets(originalRenderTargets);

		return renderTarget;
	}

	public static void RenderSpecificTiles(Vector2Int baseTilePosition, ReadOnlySpan<Vector2Int> tilePositions)
	{
		EnsureInitialized();
		Debug.Assert(tileRenderer != null);

		var tileDrawData = new TileDrawInfo();
		var screenOffset = Vector2.Zero;

		// Override renderer
		using var _1 = ValueOverride.Create(ref Main.instance.TilesRenderer, tileRenderer);
		// Adjust draw position
		using var _2 = ValueOverride.Create(ref Main.screenPosition, baseTilePosition * WorldUtils.TileSizeInPixels);
		// This hack forces Lighting.GetColor to yield with Color.White
		using var _3 = ValueOverride.Create(ref Main.gameMenu, true);
		// Get rid of scaling
		var originalZoomFactor = Main.GameViewMatrix.Zoom;
		Main.GameViewMatrix.Zoom = Vector2.One;

		ClearLegacyCachedDraws(tileRenderer);
		tileRenderer.ClearCachedTileDraws(solidLayer: false);
		tileRenderer.ClearCachedTileDraws(solidLayer: true);

		tileRenderer.PreDrawTiles(solidLayer: false, forRenderTargets: true, intoRenderTargets: true);
		Main.spriteBatch.Begin();
		
		for (int i = 0; i < tilePositions.Length; i++) {
			var tilePosition = tilePositions[i];
			var tile = Main.tile[tilePosition.X, tilePosition.Y];

			DrawSingleTile(tileRenderer, tileDrawData, true, -1, Main.screenPosition, screenOffset, tilePosition.X, tilePosition.Y);
		}

		DrawSpecialTilesLegacy(tileRenderer, Main.screenPosition, screenOffset);

		Main.spriteBatch.End();
		tileRenderer.PostDrawTiles(solidLayer: false, forRenderTargets: false, intoRenderTargets: false);

		Main.GameViewMatrix.Zoom = originalZoomFactor;
	}

	[UnsafeAccessor(UnsafeAccessorKind.Method)]
	private static extern void DrawSingleTile(TileDrawing instance, TileDrawInfo drawData, bool solidLayer, int waterStyleOverride, Vector2 screenPosition, Vector2 screenOffset, int tileX, int tileY);

	[UnsafeAccessor(UnsafeAccessorKind.Method)]
	private static extern void DrawSpecialTilesLegacy(TileDrawing instance, Vector2 screenPosition, Vector2 offSet);

	[UnsafeAccessor(UnsafeAccessorKind.Method)]
	private static extern void ClearLegacyCachedDraws(TileDrawing instance);
}

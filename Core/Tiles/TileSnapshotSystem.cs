using System;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Drawing;
using Terraria.ModLoader;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Core.Tiles;

public sealed class TileSnapshotSystem : ModSystem
{
	private delegate void DrawSingleTileDelegate(TileDrawInfo drawData, bool solidLayer, int waterStyleOverride, Vector2 screenPosition, Vector2 screenOffset, int tileX, int tileY);
	private delegate void DrawSpecialTilesLegacyDelegate(Vector2 screenPosition, Vector2 offset);

	private static DrawSingleTileDelegate? drawSingleTile;
	private static DrawSpecialTilesLegacyDelegate? drawSpecialTilesLegacy;

	public override void Load()
	{
		drawSingleTile = typeof(TileDrawing)
			.GetMethod("DrawSingleTile", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)?
			.CreateDelegate<DrawSingleTileDelegate>(Main.instance.TilesRenderer)
			?? throw new InvalidOperationException("Unable to acquire tile drawing method delegate.");

		drawSpecialTilesLegacy = typeof(TileDrawing)
			.GetMethod("DrawSpecialTilesLegacy", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)?
			.CreateDelegate<DrawSpecialTilesLegacyDelegate>(Main.instance.TilesRenderer)
			?? throw new InvalidOperationException("Unable to acquire special tile drawing method delegate.");
	}

	public static RenderTarget2D CreateSpecificTilesSnapshot(Vector2Int sizeInTiles, Vector2Int baseTilePosition, ReadOnlySpan<Vector2Int> tilePositions)
	{
		if (!Program.IsMainThread) {
			throw new InvalidOperationException($"{nameof(CreateSpecificTilesSnapshot)} can only be called on the main thread.");
		}

		var graphicsDevice = Main.graphics.GraphicsDevice;
		var originalRenderTargets = graphicsDevice.GetRenderTargets();

		var textureSize = sizeInTiles * Vector2Int.One * TileUtils.TileSizeInPixels;
		var renderTarget = new RenderTarget2D(graphicsDevice, textureSize.X, textureSize.Y, false, SurfaceFormat.Color, DepthFormat.None);

		graphicsDevice.SetRenderTarget(renderTarget);
		graphicsDevice.Clear(Color.Transparent);

		RenderSpecificTiles(baseTilePosition, tilePositions);

		graphicsDevice.SetRenderTargets(originalRenderTargets);

		return renderTarget;
	}

	public static void RenderSpecificTiles(Vector2Int baseTilePosition, ReadOnlySpan<Vector2Int> tilePositions)
	{
		Main.instance.ClearCachedTileDraws();

		var tileDrawData = new TileDrawInfo();
		var screenOffset = Vector2.Zero;
		var originalZoomFactor = Main.GameViewMatrix.Zoom;
		var originalScreenPosition = Main.screenPosition;
		bool originalGameMenu = Main.gameMenu;

		// Adjust draw position
		Main.screenPosition = baseTilePosition * TileUtils.TileSizeInPixels;
		// Get rid of scaling
		Main.GameViewMatrix.Zoom = Vector2.One;
		// This hack forces Lighting.GetColor to yield with Color.White
		Main.gameMenu = true;

		typeof(TileDrawing)
			.GetMethod("ClearLegacyCachedDraws", BindingFlags.Instance | BindingFlags.NonPublic)!
			.Invoke(Main.instance.TilesRenderer, null);

		Main.instance.TilesRenderer.PreDrawTiles(solidLayer: false, forRenderTargets: true, intoRenderTargets: true);
		Main.spriteBatch.Begin();
		
		for (int i = 0; i < tilePositions.Length; i++) {
			var tilePosition = tilePositions[i];
			var tile = Main.tile[tilePosition.X, tilePosition.Y];

			drawSingleTile!(tileDrawData, true, -1, Main.screenPosition, screenOffset, tilePosition.X, tilePosition.Y);
		}

		drawSpecialTilesLegacy!(Main.screenPosition, screenOffset);

		Main.spriteBatch.End();
		Main.instance.TilesRenderer.PostDrawTiles(solidLayer: false, forRenderTargets: false, intoRenderTargets: false);

		Main.gameMenu = originalGameMenu;
		Main.GameViewMatrix.Zoom = originalZoomFactor;
		Main.screenPosition = originalScreenPosition;
	}
}

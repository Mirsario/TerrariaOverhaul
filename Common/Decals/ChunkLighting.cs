using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoMod.RuntimeDetour;
using Terraria;
using Terraria.ModLoader;
using TerrariaOverhaul.Core.Chunks;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.Decals;

// This class provides lighting on per-chunk basis. In the future, this could be replaced with a screen-space buffer.
[Autoload(Side = ModSide.Client)]
public sealed class ChunkLighting : ChunkComponent
{
	private static uint lastLightingUpdateCount;

	public static int LightingUpdateFrequency => 10;

	public RenderTarget2D? Texture { get; private set; }
	public Surface<Color>? Colors { get; private set; }
	public bool IsReady { get; private set; }

	public override void Load()
	{
		// This fixes tileTarget not being available in many cases. And other dumb issues.
		MonoModHooks.Add(
			typeof(Main).GetProperty(nameof(Main.RenderTargetsRequired))!.GetMethod!,
			new Func<Func<bool>, bool>(orig => true)
		);
	}

	public override void OnInit(Chunk chunk)
	{
		int textureWidth = chunk.TileRectangle.Width;
		int textureHeight = chunk.TileRectangle.Height;

		Main.QueueMainThreadAction(() => {
			Colors = new Surface<Color>(textureWidth, textureHeight);
			Texture = new RenderTarget2D(Main.graphics.GraphicsDevice, textureWidth, textureHeight, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);

			Texture.InitializeWithColor(Color.Transparent); // Initialize with transparent data to prevent driver-specific issues.

			IsReady = true;
		});
	}

	public override void OnDispose(Chunk chunk)
	{
		IsReady = false;

		if (Texture != null) {
			lock (Texture) {
				var textureHandle = Texture;

				Main.QueueMainThreadAction(() => {
					textureHandle.Dispose();
				});

				Texture = null;
			}
		}
	}

	public override void PreGameDraw(Chunk chunk)
	{
		uint gameUpdateCount = Main.GameUpdateCount;

		if (gameUpdateCount != lastLightingUpdateCount && gameUpdateCount % LightingUpdateFrequency == 0) {
			UpdateLighting();

			lastLightingUpdateCount = gameUpdateCount;
		}
	}

	public void UpdateArea(Chunk chunk, Vector4Int area)
	{
		if (!IsReady) {
			throw new InvalidOperationException("Chunk lighting was not yet ready.");
		}

		for (int y = area.Y; y <= area.W; y++) {
			for (int x = area.X; x <= area.Z; x++) {
				Colors![x - chunk.TileRectangle.X, y - chunk.TileRectangle.Y] = Lighting.GetColor(x, y);
			}
		}
	}

	public void ApplyColors()
	{
		if (!IsReady) {
			throw new InvalidOperationException("Chunk lighting was not yet ready.");
		}

		lock (Texture!) {
			Texture.SetData(Colors!.Data);
		}
	}

	private static void UpdateLighting()
	{
		const int Offset = 4;

		Vector4Int loopArea;

		loopArea.X = (int)Math.Floor(Main.screenPosition.X / 16f) - Offset;
		loopArea.Y = (int)Math.Floor(Main.screenPosition.Y / 16f) - Offset;
		loopArea.Z = loopArea.X + (int)Math.Ceiling(Main.screenWidth / 16f) + Offset * 2;
		loopArea.W = loopArea.Y + (int)Math.Ceiling(Main.screenHeight / 16f) + Offset * 2;

		var chunkLoopArea = new Vector4Int(
			ChunkSystem.TileToChunkCoordinates(loopArea.X),
			ChunkSystem.TileToChunkCoordinates(loopArea.Y),
			ChunkSystem.TileToChunkCoordinates(loopArea.Z),
			ChunkSystem.TileToChunkCoordinates(loopArea.W)
		);

		for (int chunkY = chunkLoopArea.Y; chunkY <= chunkLoopArea.W; chunkY++) {
			for (int chunkX = chunkLoopArea.X; chunkX <= chunkLoopArea.Z; chunkX++) {
				if (!ChunkSystem.TryGetChunk(new Vector2Int(chunkX, chunkY), out var chunk)) {
					continue;
				}

				var lighting = chunk.Components.Get<ChunkLighting>();

				if (lighting.Texture == null) {
					continue;
				}

				lighting.UpdateArea(
					chunk,
					new Vector4Int(
						Math.Max(loopArea.X, chunk.TileRectangle.X),
						Math.Max(loopArea.Y, chunk.TileRectangle.Y),
						Math.Min(loopArea.Z, chunk.TileRectangle.Right - 1),
						Math.Min(loopArea.W, chunk.TileRectangle.Bottom - 1)
					)
				);

				lighting.ApplyColors();
			}
		}
	}
}

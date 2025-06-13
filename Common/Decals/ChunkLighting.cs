// Copyright (c) 2020-2025 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using TerrariaOverhaul.Core.Chunks;
using TerrariaOverhaul.Core.Data;
using TerrariaOverhaul.Utilities;
using TerrariaOverhaul.Utilities.Terraria;
using TerrariaOverhaul.Utilities.Xna;

namespace TerrariaOverhaul.Common.Decals;

public struct ChunkLighting : IComponent
{
	public RenderTarget2D? Texture;
	public Surface<Color>? Colors;
	public bool IsReady;
}

// This class provides lighting on per-chunk basis. In the future, this could be replaced with a screen-space buffer.
[Autoload(Side = ModSide.Client)]
public sealed class LightingSystem : ModSystem
{
	private static uint lastLightingUpdateCount;
	public static int LightingUpdateFrequency => 10;

	public override void Load()
	{
		// This fixes tileTarget not being available in many cases. And other dumb issues.
		MonoModHooks.Add(
			typeof(Main).GetProperty(nameof(Main.RenderTargetsRequired))!.GetMethod!,
			new Func<Func<bool>, bool>(orig => true)
		);

		Main.OnPreDraw += OnPreDraw;
	}
	public override void Unload()
	{
		Main.OnPreDraw -= OnPreDraw;
	}

	public static bool TryGetChunkLightingBuffer(Chunk chunk, [NotNullWhen(true)] out RenderTarget2D? result)
	{
		if (chunk.Entity.Has<ChunkLighting>() && chunk.Entity.Get<ChunkLighting>() is { IsReady: true } lighting) {
			result = lighting.Texture!;
			return true;
		}

		result = default;
		return false;
	}

	private static void OnPreDraw(GameTime obj) => TryUpdateLighting();

	private static void AddChunkComponent(Chunk chunk)
	{
		var chunkInfo = chunk.Entity.Get<ChunkInfo>();
		int textureWidth = chunkInfo.TileRectangle.Width;
		int textureHeight = chunkInfo.TileRectangle.Height;

		chunk.Entity.Add(new ChunkLighting());

		ThreadUtils.RunOnMainThread(() => {
			ref var chunkLighting = ref chunk.Entity.Get<ChunkLighting>();

			chunkLighting.Colors = new Surface<Color>(textureWidth, textureHeight);
			chunkLighting.Texture = new RenderTarget2D(Main.graphics.GraphicsDevice, textureWidth, textureHeight, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
			// Initialize with transparent data to prevent driver-specific issues.
			TextureUtils.InitializeWithColor(chunkLighting.Texture, Color.Transparent);

			chunkLighting.IsReady = true;
		});
	}
	private static void RemoveChunkComponent(Chunk chunk)
	{
		ref var chunkLighting = ref chunk.Entity.Get<ChunkLighting>();
		chunkLighting.IsReady = false;

		if (chunkLighting.Texture != null) {
			lock (chunkLighting.Texture) {
				var textureHandle = chunkLighting.Texture;
				ThreadUtils.RunOnMainThread(textureHandle.Dispose);
				chunkLighting.Texture = null;
			}
		}
	}

	private static void TryUpdateLighting()
	{
		uint gameUpdateCount = Main.GameUpdateCount;
		if (gameUpdateCount != lastLightingUpdateCount && gameUpdateCount % LightingUpdateFrequency == 0) {
			lastLightingUpdateCount = gameUpdateCount;
			UpdateLighting();
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
			Chunks.TileToChunkCoordinates(loopArea.X),
			Chunks.TileToChunkCoordinates(loopArea.Y),
			Chunks.TileToChunkCoordinates(loopArea.Z),
			Chunks.TileToChunkCoordinates(loopArea.W)
		);

		for (int chunkY = chunkLoopArea.Y; chunkY <= chunkLoopArea.W; chunkY++) {
			for (int chunkX = chunkLoopArea.X; chunkX <= chunkLoopArea.Z; chunkX++) {
				if (!Chunks.TryGetChunk(new Vector2Int(chunkX, chunkY), out var chunk)) {
					continue;
				}

				if (!chunk.Entity.Has<ChunkLighting>()) {
					AddChunkComponent(chunk);
				}

				ref var lighting = ref chunk.Entity.Get<ChunkLighting>();
				if (!lighting.IsReady) {
					continue;
				}

				ref readonly var chunkInfo = ref chunk.Entity.Get<ChunkInfo>();
				int x1 = Math.Max(loopArea.X, chunkInfo.TileRectangle.X);
				int y1 = Math.Max(loopArea.Y, chunkInfo.TileRectangle.Y);
				int x2 = Math.Min(loopArea.Z, chunkInfo.TileRectangle.Right - 1);
				int y2 = Math.Min(loopArea.W, chunkInfo.TileRectangle.Bottom - 1);

				for (int y = y1; y <= y2; y++) {
					for (int x = x1; x <= x2; x++) {
						lighting.Colors![x - chunkInfo.TileRectangle.X, y - chunkInfo.TileRectangle.Y] = Lighting.GetColor(x, y);
					}
				}

				lock (lighting.Texture!) {
					lighting.Texture.SetData(lighting.Colors!.Data);
				}
			}
		}
	}
}

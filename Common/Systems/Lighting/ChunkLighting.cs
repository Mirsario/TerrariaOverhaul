using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoMod.RuntimeDetour.HookGen;
using Terraria;
using Terraria.ModLoader;
using TerrariaOverhaul.Core.Systems.Chunks;
using TerrariaOverhaul.Utilities;
using TerrariaOverhaul.Utilities.DataStructures;

namespace TerrariaOverhaul.Common.Systems.Lighting
{
	//This class provides lighting on per-chunk basis. In the future, this could be replaced with a screen-space buffer.
	[Autoload(Side = ModSide.Client)]
	public sealed class ChunkLighting : ChunkComponent
	{
		public static int LightingUpdateFrequency => 10;

		private static volatile object lightingUpdateLock;

		public RenderTarget2D Texture { get; private set; }
		public Surface<Color> Colors { get; private set; }

		public override void Load()
		{
			lightingUpdateLock = new object();

			//This fixes tileTarget not being available in many cases. And other dumb issues.
			HookEndpointManager.Add<Func<Func<bool>, bool>>(
				typeof(Main).GetProperty(nameof(Main.RenderTargetsRequired)).GetMethod,
				new Func<Func<bool>, bool>(orig => true)
			);

			On.Terraria.Lighting.LightTiles += (On.Terraria.Lighting.orig_LightTiles orig, int firstX, int lastX, int firstY, int lastY) => {
				void UpdateLighting()
				{ 
					const int Offset = 4;

					Vector4Int loopArea;

					loopArea.X = (int)Math.Floor(Main.screenPosition.X / 16f) - Offset;
					loopArea.Y = (int)Math.Floor(Main.screenPosition.Y / 16f) - Offset;
					loopArea.Z = loopArea.X + (int)Math.Ceiling(Main.screenWidth / 16f) + Offset * 2;
					loopArea.W = loopArea.Y + (int)Math.Ceiling(Main.screenHeight / 16f) + Offset * 2;

					Vector4Int chunkLoopArea = new Vector4Int(
						ChunkSystem.TileToChunkCoordinates(loopArea.X),
						ChunkSystem.TileToChunkCoordinates(loopArea.Y),
						ChunkSystem.TileToChunkCoordinates(loopArea.Z),
						ChunkSystem.TileToChunkCoordinates(loopArea.W)
					);

					for(int chunkY = chunkLoopArea.Y; chunkY <= chunkLoopArea.W; chunkY++) {
						for(int chunkX = chunkLoopArea.X; chunkX <= chunkLoopArea.Z; chunkX++) {
							var chunk = ChunkSystem.GetOrCreateChunk(new Vector2Int(chunkX, chunkY));

							var lighting = chunk.Components.Get<ChunkLighting>();

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

				if(Main.GameUpdateCount % LightingUpdateFrequency == 0) {
					lock(lightingUpdateLock) {
						UpdateLighting();
					}
				}

				orig(firstX, lastX, firstY, lastY);
			};
		}
		
		public override void Unload()
		{
			lightingUpdateLock = null;
		}

		public override void OnInit(Chunk chunk)
		{
			int textureWidth = chunk.TileRectangle.Width;
			int textureHeight = chunk.TileRectangle.Height;

			Colors = new Surface<Color>(textureWidth, textureHeight);
			Texture = new RenderTarget2D(Main.graphics.GraphicsDevice, textureWidth, textureHeight, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
		}
		
		public override void OnDispose(Chunk chunk)
		{
			if(Texture != null) {
				lock(Texture) {
					Texture.Dispose();

					Texture = null;
				}
			}
		}

		public void UpdateArea(Chunk chunk, Vector4Int area)
		{
			for(int y = area.Y; y <= area.W; y++) {
				for(int x = area.X; x <= area.Z; x++) {
					Colors[x - chunk.TileRectangle.X, y - chunk.TileRectangle.Y] = Terraria.Lighting.GetColor(x, y);
				}
			}
		}
		public void ApplyColors()
		{
			lock(Texture) {
				Texture.SetData(Colors.Data);
			}
		}
	}
}

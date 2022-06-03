using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Core.Chunks
{
	public class ChunkSystem : ModSystem
	{
		public const int ChunkUpdateArea = 3;

		private static Dictionary<long, Chunk>? chunks;

		public override void Load()
		{
			chunks = new Dictionary<long, Chunk>();

			Main.OnPreDraw += OnPreDraw;
		}

		public override void Unload()
		{
			if (chunks != null) {
				foreach (var chunk in chunks.Values) {
					chunk.Dispose();
				}

				chunks.Clear();

				chunks = null;
			}

			Main.OnPreDraw -= OnPreDraw;
		}

		public override void PostDrawTiles()
		{
			if (Main.gameMenu) {
				return;
			}

			var sb = Main.spriteBatch;

			//sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

			foreach (var chunk in EnumerateChunksInArea(Main.LocalPlayer, ChunkUpdateArea, false)) {
				foreach (var component in chunk.Components) {
					component.PostDrawTiles(chunk, sb);
				}
			}

			//sb.End();
		}

		/*public override void PostUpdateEverything()
		{
			foreach(var chunk in EnumerateChunksInArea(Main.LocalPlayer, ChunkUpdateArea, false)) {
				foreach(var component in chunk.EnumerateComponents()) {
					component.OnUpdate();
				}
			}
		}*/

		private void OnPreDraw(GameTime gameTime)
		{
			if (Main.gameMenu) {
				return;
			}

			foreach (var chunk in EnumerateChunksInArea(Main.LocalPlayer, ChunkUpdateArea, false)) {
				foreach (var component in chunk.Components) {
					component.PreGameDraw(chunk);
				}
			}
		}

		public static IEnumerable<Chunk> EnumerateChunksInArea(Player player, int areaSize, bool instantiate)
		{
			if (player?.active != true) {
				return Enumerable.Empty<Chunk>();
			}

			return EnumerateChunksInArea(player.position.ToTileCoordinates(), areaSize, instantiate);
		}

		public static IEnumerable<Chunk> EnumerateChunksInArea(Vector2Int tileCenter, int areaSize, bool instantiate)
		{
			if (chunks == null) {
				throw new InvalidOperationException("Chunks are not initialized.");
			}

			Vector2Int chunkCenter = TileToChunkCoordinates(tileCenter);

			int xStart = chunkCenter.X - areaSize;
			int yStart = chunkCenter.Y - areaSize;
			int xEnd = chunkCenter.X + areaSize;
			int yEnd = chunkCenter.Y + areaSize;

			for (int y = yStart; y < yEnd; y++) {
				for (int x = xStart; x < xEnd; x++) {
					long encodedPosition = Chunk.PackPosition(x, y);

					if (!chunks.TryGetValue(encodedPosition, out var chunk)) {
						if (!instantiate) {
							continue;
						}

						chunks[encodedPosition] = chunk = new Chunk(x, y);
					}

					yield return chunk;
				}
			}
		}

		// TryGet
		public static bool TryGetChunkAtWorldPosition(Vector2 worldPosition, out Chunk chunk)
			=> TryGetChunkAtTilePosition(worldPosition.ToTileCoordinates(), out chunk);

		public static bool TryGetChunkAtTilePosition(Vector2Int tilePosition, out Chunk chunk)
			=> TryGetChunk(TileToChunkCoordinates(tilePosition), out chunk);

		public static bool TryGetChunk(Vector2Int chunkPosition, out Chunk chunk)
		{
			if (chunks == null) {
				chunk = null!;
				
				return false;
			}

			return chunks.TryGetValue(Chunk.PackPosition(chunkPosition.X, chunkPosition.Y), out chunk!);
		}

		// GetOrCreate
		public static Chunk GetOrCreateChunkAtWorldPosition(Vector2 worldPosition)
			=> GetOrCreateChunkAtTilePosition(TileToChunkCoordinates(worldPosition.ToTileCoordinates()));

		public static Chunk GetOrCreateChunkAtTilePosition(Vector2Int tilePosition)
			=> GetOrCreateChunk(TileToChunkCoordinates(tilePosition));

		public static Chunk GetOrCreateChunk(Vector2Int chunkPosition)
		{
			if (chunks == null) {
				throw new InvalidOperationException("Chunks are not initialized.");
			}

			long encodedPosition = Chunk.PackPosition(chunkPosition.X, chunkPosition.Y);

			if (!chunks.TryGetValue(encodedPosition, out var chunk)) {
				chunks[encodedPosition] = chunk = new Chunk(chunkPosition.X, chunkPosition.Y);
			}

			return chunk;
		}

		public static int TileToChunkCoordinates(int coordinate)
			=> coordinate / Chunk.MaxChunkSize;

		public static Vector2Int TileToChunkCoordinates(Vector2Int tilePosition)
			=> new(tilePosition.X / Chunk.MaxChunkSize, tilePosition.Y / Chunk.MaxChunkSize);
	}
}

using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using TerrariaOverhaul.Utilities.DataStructures;

namespace TerrariaOverhaul.Core.Systems.Chunks
{
	public class ChunkSystem : ModSystem
	{
		public const int ChunkUpdateArea = 3;

		internal static readonly List<ChunkComponent> ChunkComponents = new List<ChunkComponent>();

		private static Dictionary<long, Chunk> chunks;

		public override void Load()
		{
			chunks = new Dictionary<long, Chunk>();

			Main.OnPreDraw += OnPreDraw;
		}
		public override void Unload()
		{
			ChunkComponents.Clear();

			if(chunks != null) {
				foreach(var chunk in chunks.Values) {
					chunk.Dispose();
				}

				chunks.Clear();

				chunks = null;
			}

			Main.OnPreDraw -= OnPreDraw;
		}

		public override void PostDrawTiles()
		{
			var sb = Main.spriteBatch;

			//sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

			foreach(var chunk in EnumerateChunksInArea(Main.LocalPlayer, ChunkUpdateArea, false)) {
				foreach(var component in chunk.EnumerateComponents()) {
					component.PostDrawTiles(sb);
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
			foreach(var chunk in EnumerateChunksInArea(Main.LocalPlayer, ChunkUpdateArea, false)) {
				foreach(var component in chunk.EnumerateComponents()) {
					component.PreGameDraw();
				}
			}
		}

		public static IEnumerable<Chunk> EnumerateChunksInArea(Player player, int areaSize, bool instantiate)
		{
			if(player?.active != true) {
				return Enumerable.Empty<Chunk>();
			}

			return EnumerateChunksInArea(player.position.ToTileCoordinates(), areaSize, instantiate);
		}
		public static IEnumerable<Chunk> EnumerateChunksInArea(Vector2Int tileCenter, int areaSize, bool instantiate)
		{
			Vector2Int chunkCenter = TileToChunkCoordinates(tileCenter);

			int xStart = chunkCenter.X - areaSize;
			int yStart = chunkCenter.Y - areaSize;
			int xEnd = chunkCenter.X + areaSize;
			int yEnd = chunkCenter.Y + areaSize;

			for(int y = yStart; y < yEnd; y++) {
				for(int x = xStart; x < xEnd; x++) {
					long encodedPosition = Chunk.PackPosition(x, y);

					if(!chunks.TryGetValue(encodedPosition, out var chunk)) {
						if(!instantiate) {
							continue;
						}

						chunks[encodedPosition] = chunk = new Chunk(x, y);
					}

					yield return chunk;
				}
			}
		}
		//TryGet
		public static bool TryGetChunkAtWorldPosition(Vector2 worldPosition, out Chunk chunk) => TryGetChunkAtTilePosition(worldPosition.ToTileCoordinates(), out chunk);
		public static bool TryGetChunkAtTilePosition(Vector2Int tilePosition, out Chunk chunk) => TryGetChunk(TileToChunkCoordinates(tilePosition), out chunk);
		public static bool TryGetChunk(Vector2Int chunkPosition, out Chunk chunk) => chunks.TryGetValue(Chunk.PackPosition(chunkPosition.X, chunkPosition.Y), out chunk);
		//GetOrCreate
		public static Chunk GetOrCreateChunkAtWorldPosition(Vector2 worldPosition) => GetOrCreateChunkAtTilePosition(TileToChunkCoordinates(worldPosition.ToTileCoordinates()));
		public static Chunk GetOrCreateChunkAtTilePosition(Vector2Int tilePosition) => GetOrCreateChunk(TileToChunkCoordinates(tilePosition));
		public static Chunk GetOrCreateChunk(Vector2Int chunkPosition)
		{
			long encodedPosition = Chunk.PackPosition(chunkPosition.X, chunkPosition.Y);

			if(!chunks.TryGetValue(encodedPosition, out var chunk)) {
				chunks[encodedPosition] = chunk = new Chunk(chunkPosition.X, chunkPosition.Y);
			}

			return chunk;
		}

		internal static int RegisterComponent(ChunkComponent component)
		{
			ChunkComponents.Add(component);

			return ChunkComponents.Count - 1;
		}

		public static int TileToChunkCoordinates(int coordinate) => coordinate / Chunk.MaxChunkSize;
		public static Vector2Int TileToChunkCoordinates(Vector2Int tilePosition) => new Vector2Int(tilePosition.X / Chunk.MaxChunkSize, tilePosition.Y / Chunk.MaxChunkSize);
	}
}

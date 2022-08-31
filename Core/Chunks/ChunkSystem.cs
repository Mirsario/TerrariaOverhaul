using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Core.Chunks;

public class ChunkSystem : ModSystem
{
	public const int ChunkUpdateArea = 3;

	private static Dictionary<long, Chunk>? chunks;

	public static Vector2Int WorldSize => new(Main.maxTilesX, Main.maxTilesY);
	public static Vector2Int WorldSizeInChunks => new(
		(int)MathF.Ceiling(Main.maxTilesX / (float)Chunk.MaxChunkSize),
		(int)MathF.Ceiling(Main.maxTilesY / (float)Chunk.MaxChunkSize)
	);

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

		foreach (var chunk in EnumerateChunksInArea(Main.LocalPlayer, ChunkUpdateArea)) {
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

		foreach (var chunk in EnumerateChunksInArea(Main.LocalPlayer, ChunkUpdateArea)) {
			foreach (var component in chunk.Components) {
				component.PreGameDraw(chunk);
			}
		}
	}

	public static IEnumerable<Chunk> EnumerateChunksInArea(Player player, int areaSize)
	{
		if (player?.active != true) {
			return Enumerable.Empty<Chunk>();
		}

		return EnumerateChunksInArea(player.position.ToTileCoordinates(), areaSize);
	}

	public static IEnumerable<Chunk> EnumerateChunksInArea(Vector2Int tileCenter, int areaSize)
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
				if (TryGetChunk(new Vector2Int(x, y), out var chunk)) {
					yield return chunk;
				}
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
	public static bool TryGetOrCreateChunkAtWorldPosition(Vector2 worldPosition, [NotNullWhen(true)] out Chunk? chunk)
		=> TryGetOrCreateChunkAtTilePosition(TileToChunkCoordinates(worldPosition.ToTileCoordinates()), out chunk);

	public static bool TryGetOrCreateChunkAtTilePosition(Vector2Int tilePosition, [NotNullWhen(true)] out Chunk? chunk)
		=> TryGetOrCreateChunk(TileToChunkCoordinates(tilePosition), out chunk);

	public static bool TryGetOrCreateChunk(Vector2Int chunkPosition, [NotNullWhen(true)] out Chunk? chunk)
	{
		if (chunks == null) {
			throw new InvalidOperationException("Chunks are not initialized.");
		}

		if (chunkPosition.X < 0 || chunkPosition.Y < 0) {
			chunk = null;

			return false;
		}

		var worldSizeInChunks = WorldSizeInChunks;

		if (chunkPosition.X >= worldSizeInChunks.X || chunkPosition.Y >= worldSizeInChunks.Y) {
			chunk = null;

			return false;
		}

		long encodedPosition = Chunk.PackPosition(chunkPosition.X, chunkPosition.Y);

		if (!chunks.TryGetValue(encodedPosition, out chunk)) {
			chunks[encodedPosition] = chunk = new Chunk(chunkPosition.X, chunkPosition.Y);
		}
		
		return true;
	}

	public static int TileToChunkCoordinates(int coordinate)
		=> coordinate / Chunk.MaxChunkSize;

	public static Vector2Int TileToChunkCoordinates(Vector2Int tilePosition)
		=> new(tilePosition.X / Chunk.MaxChunkSize, tilePosition.Y / Chunk.MaxChunkSize);
}

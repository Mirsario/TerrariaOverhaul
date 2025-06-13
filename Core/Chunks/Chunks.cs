// Copyright (c) 2020-2025 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using TerrariaOverhaul.Core.Data;
using TerrariaOverhaul.Utilities.Terraria;
using TerrariaOverhaul.Utilities.Xna;

namespace TerrariaOverhaul.Core.Chunks;

public readonly struct Chunk(DataEntity entity)
{
	public readonly DataEntity Entity = entity;
}

public readonly struct ChunkInfo : IComponent
{
	public readonly Vector2Int Position;
	public readonly long EncodedPosition;
	public readonly RectFloat Rectangle;
	public readonly Rectangle TileRectangle;
	public readonly RectFloat WorldRectangle;

	public ChunkInfo(int x, int y)
	{
		Position = new Vector2Int(x, y);
		EncodedPosition = Chunks.PackPosition(x, y);

		int xTilePos = Position.X * Chunks.MaxChunkSize;
		int yTilePos = Position.Y * Chunks.MaxChunkSize;

		TileRectangle = new Rectangle(
			xTilePos,
			yTilePos,
			Math.Min(Main.maxTilesX, xTilePos + Chunks.MaxChunkSize) - xTilePos,
			Math.Min(Main.maxTilesY, yTilePos + Chunks.MaxChunkSize) - yTilePos
		);
		Rectangle = new RectFloat(
			TileRectangle.X / (float)Chunks.MaxChunkSize,
			TileRectangle.Y / (float)Chunks.MaxChunkSize,
			TileRectangle.Width / (float)Chunks.MaxChunkSize,
			TileRectangle.Height / (float)Chunks.MaxChunkSize
		);
		WorldRectangle = new RectFloat(
			TileRectangle.X * WorldUtils.TileSizeInPixels,
			TileRectangle.Y * WorldUtils.TileSizeInPixels,
			TileRectangle.Width * WorldUtils.TileSizeInPixels,
			TileRectangle.Height * WorldUtils.TileSizeInPixels
		);
	}
}

public class Chunks : ModSystem
{
	public const int MaxChunkSize = 64;
	public const float MaxChunkSizeInPixels = MaxChunkSize * WorldUtils.TileSizeInPixels;

	private static Dictionary<long, Chunk>? chunks;

	public static Vector2Int WorldSize => new(Main.maxTilesX, Main.maxTilesY);
	public static Vector2Int WorldSizeInChunks => new(
		(int)MathF.Ceiling(Main.maxTilesX / (float)MaxChunkSize),
		(int)MathF.Ceiling(Main.maxTilesY / (float)MaxChunkSize)
	);

	public override void Load()
	{
		chunks = new Dictionary<long, Chunk>();
	}
	public override void Unload()
	{
		if (chunks != null) {
			//foreach (var chunk in chunks.Values) chunk.Dispose();
			chunks.Clear();
			chunks = null;
		}
	}

	public static IEnumerable<Chunk> IterateAllChunks()
		=> chunks != null ? chunks.Values : Enumerable.Empty<Chunk>();

	public static IEnumerable<Chunk> IterateVisibleChunks()
	{
		return IterateChunksInWorldArea(new Vector4(
			Main.screenPosition.X,
			Main.screenPosition.Y,
			Main.screenPosition.X + Main.screenWidth,
			Main.screenPosition.X + Main.screenHeight
		));
	}
	public static IEnumerable<Chunk> IterateChunksInWorldArea(Vector4 worldArea)
	{
		return IterateChunksInChunkArea(new Vector4Int(
			WorldToChunkCoordinates(worldArea.X),
			WorldToChunkCoordinates(worldArea.Y),
			WorldToChunkCoordinates(worldArea.Z),
			WorldToChunkCoordinates(worldArea.W)
		));
	}
	public static IEnumerable<Chunk> IterateChunksInTileArea(Vector4Int tileArea)
	{
		return IterateChunksInChunkArea(new Vector4Int(
			TileToChunkCoordinates(tileArea.X),
			TileToChunkCoordinates(tileArea.Y),
			TileToChunkCoordinates(tileArea.Z),
			TileToChunkCoordinates(tileArea.W)
		));
	}
	public static IEnumerable<Chunk> IterateChunksInChunkArea(Vector4Int chunkArea)
	{
		if (chunks == null) yield break;

		for (int y = chunkArea.Y; y <= chunkArea.W; y++) {
			for (int x = chunkArea.X; x <= chunkArea.Z; x++) {
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
			chunk = default;
			return false;
		}

		return chunks.TryGetValue(PackPosition(chunkPosition.X, chunkPosition.Y), out chunk!);
	}

	// GetOrCreate
	public static bool TryGetOrCreateChunkAtWorldPosition(Vector2 worldPosition, [NotNullWhen(true)] out Chunk chunk)
		=> TryGetOrCreateChunkAtTilePosition(TileToChunkCoordinates(worldPosition.ToTileCoordinates()), out chunk);

	public static bool TryGetOrCreateChunkAtTilePosition(Vector2Int tilePosition, [NotNullWhen(true)] out Chunk chunk)
		=> TryGetOrCreateChunk(TileToChunkCoordinates(tilePosition), out chunk);

	public static bool TryGetOrCreateChunk(Vector2Int chunkPosition, [NotNullWhen(true)] out Chunk chunk)
	{
		if (chunks == null) {
			throw new InvalidOperationException("Chunks are not initialized.");
		}

		if (chunkPosition.X < 0 || chunkPosition.Y < 0) {
			chunk = default;
			return false;
		}

		var worldSizeInChunks = WorldSizeInChunks;

		if (chunkPosition.X >= worldSizeInChunks.X || chunkPosition.Y >= worldSizeInChunks.Y) {
			chunk = default;
			return false;
		}

		long encodedPosition = PackPosition(chunkPosition.X, chunkPosition.Y);
		if (!chunks.TryGetValue(encodedPosition, out chunk)) {
			chunks[encodedPosition] = chunk = new(DataStorage.CreateEntity());
			chunk.Entity.Add(new ChunkInfo(chunkPosition.X, chunkPosition.Y));
		}
		
		return true;
	}

	// Coordinates

	public static int WorldToChunkCoordinates(float coordinate)
		=> (int)(coordinate / (WorldUtils.TileSizeInPixels * MaxChunkSize));
	public static int TileToChunkCoordinates(int coordinate)
		=> coordinate / MaxChunkSize;
	public static Vector2Int TileToChunkCoordinates(Vector2Int tilePosition)
		=> new(tilePosition.X / MaxChunkSize, tilePosition.Y / MaxChunkSize);

	public static long PackPosition(int x, int y)
		=> ((long)y << 32) | (uint)x;
}

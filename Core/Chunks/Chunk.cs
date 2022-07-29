using System;
using Microsoft.Xna.Framework;
using Terraria;
using TerrariaOverhaul.Core.Components;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Core.Chunks;

public sealed class Chunk : IDisposable
{
	public const int MaxChunkSize = 64;
	public const float MaxChunkSizeInPixels = MaxChunkSize * TileUtils.TileSizeInPixels;

	public readonly Vector2Int Position;
	public readonly long EncodedPosition;
	public readonly RectFloat Rectangle;
	public readonly Rectangle TileRectangle;
	public readonly RectFloat WorldRectangle;
	public readonly ModComponentContainer<Chunk, ChunkComponent> Components;

	internal Chunk(int x, int y)
	{
		// Positions

		Position = new Vector2Int(x, y);
		EncodedPosition = PackPosition(x, y);

		int xTilePos = Position.X * MaxChunkSize;
		int yTilePos = Position.Y * MaxChunkSize;

		TileRectangle = new Rectangle(
			xTilePos,
			yTilePos,
			Math.Min(Main.maxTilesX, xTilePos + MaxChunkSize) - xTilePos,
			Math.Min(Main.maxTilesY, yTilePos + MaxChunkSize) - yTilePos
		);

		Rectangle = new RectFloat(
			TileRectangle.X / (float)MaxChunkSize,
			TileRectangle.Y / (float)MaxChunkSize,
			TileRectangle.Width / (float)MaxChunkSize,
			TileRectangle.Height / (float)MaxChunkSize
		);

		WorldRectangle = new RectFloat(
			TileRectangle.X * TileUtils.TileSizeInPixels,
			TileRectangle.Y * TileUtils.TileSizeInPixels,
			TileRectangle.Width * TileUtils.TileSizeInPixels,
			TileRectangle.Height * TileUtils.TileSizeInPixels
		);

		// Components

		Components = new(this);
	}

	public void Dispose()
	{
		Components.Dispose();
	}

	public static long PackPosition(int x, int y)
	{
		return ((long)y << 32) | (uint)x;
	}
}

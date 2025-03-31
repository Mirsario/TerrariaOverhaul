// Copyright (c) 2020-2025 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using System;
using Microsoft.Xna.Framework;
using Terraria;
using TerrariaOverhaul.Core.Components;
using TerrariaOverhaul.Utilities.Terraria;
using TerrariaOverhaul.Utilities.Xna;

namespace TerrariaOverhaul.Core.Chunks;

public sealed class Chunk : IDisposable
{
	public const int MaxChunkSize = 64;
	public const float MaxChunkSizeInPixels = MaxChunkSize * WorldUtils.TileSizeInPixels;

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
			TileRectangle.X * WorldUtils.TileSizeInPixels,
			TileRectangle.Y * WorldUtils.TileSizeInPixels,
			TileRectangle.Width * WorldUtils.TileSizeInPixels,
			TileRectangle.Height * WorldUtils.TileSizeInPixels
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

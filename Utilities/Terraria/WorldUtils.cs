// Copyright (c) 2020-2025 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using System;
using Terraria.DataStructures;
using Terraria;
using Microsoft.Xna.Framework;
using System.Runtime.CompilerServices;
using TerrariaOverhaul.Utilities.Xna;

namespace TerrariaOverhaul.Utilities.Terraria;

internal static class WorldUtils
{
	public const int TileSizeInPixels = 16;
	public const float PixelSizeInUnits = 1f / TileSizeInPixels;

	// Boundary Checks

	public static bool IsInWorld(this Point point)
		=> point.X >= 0 && point.Y >= 0 && point.X < Main.maxTilesX && point.Y < Main.maxTilesY;
	public static bool IsInWorld(this Point16 point)
		=> point.X >= 0 && point.Y >= 0 && point.X < Main.maxTilesX && point.Y < Main.maxTilesY;
	public static bool IsInWorld(this Vector2 vec)
		=> vec.X > 0 && vec.Y > 0 && vec.X < (Main.maxTilesX - 1) * WorldUtils.TileSizeInPixels && vec.Y < (Main.maxTilesY - 1) * WorldUtils.TileSizeInPixels;

	// Conversions

	public static Rectangle ToTileCoordinates(this Rectangle rect)
		=> new(rect.X / TileSizeInPixels, rect.Y / TileSizeInPixels, rect.Width / TileSizeInPixels, rect.Height / TileSizeInPixels);

	public static Rectangle ToWorldCoordinates(this Rectangle rect)
		=> new(rect.X * TileSizeInPixels, rect.Y * TileSizeInPixels, rect.Width * TileSizeInPixels, rect.Height * TileSizeInPixels);

	// Clamping

	public static Vector4Int ClampTileCoordinates(Vector4Int xyzw)
	{
		Vector4Int result;
		result.X = Math.Min(Math.Max(xyzw.X, 0), Main.maxTilesX);
		result.Y = Math.Min(Math.Max(xyzw.Y, 0), Main.maxTilesY);
		result.Z = Math.Min(Math.Max(xyzw.Z, 0), Main.maxTilesX);
		result.W = Math.Min(Math.Max(xyzw.W, 0), Main.maxTilesY);
		return result;
	}
	public static Rectangle ClampTileCoordinates(Rectangle rect)
	{
		int x = Math.Min(Math.Max(rect.Left, 0), Main.maxTilesX);
		int y = Math.Min(Math.Max(rect.Top, 0), Main.maxTilesY);
		int z = Math.Min(Math.Max(rect.Right, 0), Main.maxTilesX);
		int w = Math.Min(Math.Max(rect.Bottom, 0), Main.maxTilesY);
		return new(x, y, z - x, w - y);
	}
	public static Vector4 ClampWorldCoordinates(Vector4 xyzw)
	{
		Vector4 result;
		result.X = Math.Min(Math.Max(xyzw.X, Main.leftWorld), Main.rightWorld);
		result.Y = Math.Min(Math.Max(xyzw.Y, Main.topWorld), Main.bottomWorld);
		result.Z = Math.Min(Math.Max(xyzw.Z, Main.leftWorld), Main.rightWorld);
		result.W = Math.Min(Math.Max(xyzw.W, Main.topWorld), Main.bottomWorld);
		return result;
	}
	public static Rectangle ClampWorldCoordinates(Rectangle rect)
	{
		int x = Math.Min(Math.Max(rect.Left, 0), (int)Main.rightWorld);
		int y = Math.Min(Math.Max(rect.Top, 0), (int)Main.bottomWorld);
		int z = Math.Min(Math.Max(rect.Right, 0), (int)Main.rightWorld);
		int w = Math.Min(Math.Max(rect.Bottom, 0), (int)Main.bottomWorld);
		return new(x, y, z - x, w - y);
	}

	// Tilemap Getters

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Tile Get(this Tilemap tilemap, Point16 pos)
		=> tilemap[pos.X, pos.Y];

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Tile Get(this Tilemap tilemap, Vector2Int pos)
		=> tilemap[pos.X, pos.Y];

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool TryGet(this Tilemap tilemap, Point16 pos, out Tile tile)
		=> TryGet(tilemap, pos.X, pos.Y, out tile);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool TryGet(this Tilemap tilemap, Vector2Int pos, out Tile tile)
		=> TryGet(tilemap, pos.X, pos.Y, out tile);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool TryGet(this Tilemap tilemap, int x, int y, out Tile tile)
	{
		if (x >= 0 && y >= 0 && x < Main.maxTilesX && y < Main.maxTilesY) {
			tile = tilemap[x, y];
			return true;
		}

		tile = default;
		return false;
	}

	// Tile Checks

	public static bool CheckIfAllBlocksAreSolid(int x, int y, int width, int height)
	{
		var xyzw = ClampTileCoordinates(new Vector4Int(x, y, x + width, y + height));
		for (int yy = xyzw.Y; yy < xyzw.W; yy++) {
			for (int xx = xyzw.X; xx < xyzw.Z; xx++) {
				var tile = Main.tile[xx, yy];
				if (!Main.tileSolid[tile.TileType] || !tile.HasTile)
					return false;
			}
		}

		return true;
	}

	public static bool CheckAreaAll(int x, int y, int width, int height, Func<int, int, Tile, bool> func)
	{
		int x1 = (int)MathHelper.Clamp(x, 0, Main.maxTilesX - 1);
		int y1 = (int)MathHelper.Clamp(y, 0, Main.maxTilesY - 1);
		int x2 = (int)MathHelper.Clamp(x + width, 0, Main.maxTilesX - 1);
		int y2 = (int)MathHelper.Clamp(y + height, 0, Main.maxTilesY - 1);

		for (int yy = y1; yy < y2; yy++) {
			for (int xx = x1; xx < x2; xx++) {
				if (!Main.tile.TryGet(xx, yy, out var tile) || !func(xx, yy, tile)) {
					return false;
				}
			}
		}

		return true;
	}

	public static bool CheckDiamondAll(int x, int y, Func<Tile, Point16, bool> func)
	{
		for (int yy = -1; yy <= 1; yy++) {
			for (int xx = -1; xx <= 1; xx++) {
				if (Math.Abs(xx) == Math.Abs(yy)) {
					continue;
				}

				var point = new Point16(x + xx, y + yy);
				if (!Main.tile.TryGet(point, out var tile) || !func(tile, point)) {
					return false;
				}
			}
		}

		return true;
	}

	public static bool CheckSurrounded(int x, int y)
		=> CheckDiamondAll(x, y, (tile, point) => tile.HasTile && Main.tileSolid[tile.TileType]);

	public static bool CheckTotallySurrounded(int x, int y)
		=> CheckDiamondAll(x, y, (tile, point) => tile.HasTile && Main.tileSolid[tile.TileType] && !Main.tileSolidTop[tile.TileType]);
}

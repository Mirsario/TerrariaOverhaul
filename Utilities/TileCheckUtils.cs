using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.DataStructures;
using TerrariaOverhaul.Utilities.Extensions;

namespace TerrariaOverhaul.Utilities
{
	public static class TileCheckUtils
	{
		public static bool CheckIfAllBlocksAreSolid(int x, int y, int width, int height)
		{
			for(int yy = 0; yy < height; yy++) {
				for(int xx = 0; xx < width; xx++) {
					if(!Main.tile.TryGet(x + xx, y + yy, out Tile tile) || !Main.tileSolid[tile.type] || !tile.IsActive) {
						return false;
					}
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

			for(int yy = y1; yy < y2; yy++) {
				for(int xx = x1; xx < x2; xx++) {
					if(!Main.tile.TryGet(xx, yy, out var tile) || !func(xx, yy, tile)) {
						return false;
					}
				}
			}

			return true;
		}

		public static bool CheckDiamondAll(int x, int y, Func<Tile, Point16, bool> func)
		{
			for(int yy = -1; yy <= 1; yy++) {
				for(int xx = -1; xx <= 1; xx++) {
					if(Math.Abs(xx) == Math.Abs(yy)) {
						continue;
					}

					var point = new Point16(x + xx, y + yy);

					if(!Main.tile.TryGet(point, out var tile) || !func(tile, point)) {
						return false;
					}
				}
			}

			return true;
		}

		public static bool CheckSurrounded(int x, int y)
			=> CheckDiamondAll(x, y, (tile, point) => tile.IsActive && Main.tileSolid[tile.type]);

		public static bool CheckTotallySurrounded(int x, int y)
			=> CheckDiamondAll(x, y, (tile, point) => tile.IsActive && Main.tileSolid[tile.type] && !Main.tileSolidTop[tile.type]);
	}
}

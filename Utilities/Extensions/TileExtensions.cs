using Terraria;
using Terraria.DataStructures;
using TerrariaOverhaul.Utilities.DataStructures;

namespace TerrariaOverhaul.Utilities.Extensions
{
	public static class TileExtensions
	{
		public static bool TryGet(this Tile[,] tiles, Point16 pos, out Tile tile) => TryGet(tiles, pos.X, pos.Y, out tile);
		public static bool TryGet(this Tile[,] tiles, Vector2Int pos, out Tile tile) => TryGet(tiles, pos.X, pos.Y, out tile);
		public static bool TryGet(this Tile[,] tiles, int x, int y, out Tile tile)
		{
			if(x >= 0 && y >= 0 && x < Main.maxTilesX && y < Main.maxTilesY) {
				return (tile = tiles[x, y]) != null;
			}

			tile = null;

			return false;
		}
	}
}

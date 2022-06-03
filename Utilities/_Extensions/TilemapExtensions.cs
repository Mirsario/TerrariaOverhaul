using Terraria;
using Terraria.DataStructures;

namespace TerrariaOverhaul.Utilities
{
	public static class TilemapExtensions
	{
		public static bool TryGet(this Tilemap tilemap, Point16 pos, out Tile tile)
			=> TryGet(tilemap, pos.X, pos.Y, out tile);

		public static bool TryGet(this Tilemap tilemap, Vector2Int pos, out Tile tile)
			=> TryGet(tilemap, pos.X, pos.Y, out tile);

		public static bool TryGet(this Tilemap tilemap, int x, int y, out Tile tile)
		{
			if (x >= 0 && y >= 0 && x < Main.maxTilesX && y < Main.maxTilesY) {
				return (tile = tilemap[x, y]) != null;
			}

			tile = default;

			return false;
		}
	}
}

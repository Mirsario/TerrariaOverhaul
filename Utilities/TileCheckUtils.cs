using Terraria;
using TerrariaOverhaul.Utilities.Extensions;

namespace TerrariaOverhaul.Utilities
{
	public static class TileCheckUtils
	{
		public static bool CheckIfAllBlocksAreSolid(int x, int y, int width, int height)
		{
			for(int yy = 0; yy < height; yy++) {
				for(int xx = 0; xx < width; xx++) {
					if(!Main.tile.TryGet(x + xx, y + yy, out Tile tile) || !Main.tileSolid[tile.type] || !tile.active()) {
						return false;
					}
				}
			}

			return true;
		}
	}
}

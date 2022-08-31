using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.Decals;

[Autoload(Side = ModSide.Client)]
public class DecalTiles : GlobalTile
{
	public override bool TileFrame(int x, int y, int type, ref bool resetFrame, ref bool noBreak)
	{
		if (!WorldGen.gen && TileLoader.CloseDoorID(Main.tile[x, y]) >= 0) {
			DecalSystem.ClearDecals(new Rectangle(x * 16, y * 16, 16, 16));
		}

		return true;
	}

	public override void KillTile(int x, int y, int type, ref bool fail, ref bool effectOnly, ref bool noItem)
	{
		if (!WorldGen.gen && !effectOnly) {
			DecalSystem.ClearDecals(new Rectangle(x * 16, y * 16, 16, 16));
		}
	}

	public override void PlaceInWorld(int x, int y, int type, Item item)
	{
		if (WorldGen.gen) {
			return;
		}

		for (int yy = -1; yy <= 1; yy++) {
			for (int xx = -1; xx <= 1; xx++) {
				var point = new Point(x + xx, y + yy);

				if (Main.tile.TryGet(point, out var tile) && tile.HasTile && TileCheckUtils.CheckSurrounded(point.X, point.Y)) {
					DecalSystem.ClearDecals(new Rectangle(point.X * 16, point.Y * 16, 16, 16));
				}
			}
		}
	}
}

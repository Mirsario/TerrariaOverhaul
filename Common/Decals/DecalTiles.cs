using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

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
		if (!WorldGen.gen && !fail && !effectOnly) {
			DecalSystem.ClearDecals(new Rectangle(x * 16, y * 16, 16, 16));
		}
	}

	public override void PlaceInWorld(int x, int y, int type, Item item)
	{
		if (!WorldGen.gen) {
			DecalSystem.ClearDecals(new Rectangle(x * 16, y * 16, 16, 16));
		}
	}
}

// Copyright (c) 2020-2026 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using TerrariaOverhaul.Utilities.Terraria;
using TerrariaOverhaul.Utilities.Xna;

namespace TerrariaOverhaul.Common.Decals;

[Autoload(Side = ModSide.Client)]
internal class DecalTiles : GlobalTile
{
	public override bool TileFrame(int x, int y, int type, ref bool resetFrame, ref bool noBreak)
	{
		if (!WorldGen.gen && TileLoader.CloseDoorID(Main.tile[x, y]) >= 0) {
			DecalSystem.ClearDecals(new Rectangle(x * WorldUtils.TileSizeInPixels, y * WorldUtils.TileSizeInPixels, WorldUtils.TileSizeInPixels, WorldUtils.TileSizeInPixels));
		}

		return true;
	}

	public override void KillTile(int x, int y, int type, ref bool fail, ref bool effectOnly, ref bool noItem)
	{
		if (!WorldGen.gen && !effectOnly) {
			DecalSystem.ClearDecals(new Rectangle(x * WorldUtils.TileSizeInPixels, y * WorldUtils.TileSizeInPixels, WorldUtils.TileSizeInPixels, WorldUtils.TileSizeInPixels));
		}
	}

	public override void PlaceInWorld(int x, int y, int type, Item item)
	{
		if (WorldGen.gen)
			return;

		var xyzw = WorldUtils.ClampTileCoordinates(new Vector4Int(x - 1, y - 1, x + 1, y + 1));
		for (int yy = xyzw.Y; yy <= xyzw.W; yy++) {
			for (int xx = xyzw.X; xx <= xyzw.Z; xx++) {
				var tile = Main.tile[xx, yy];
				if (tile.HasTile && WorldUtils.CheckSurrounded(xx, yy)) {
					DecalSystem.ClearDecals(new Rectangle(xx * WorldUtils.TileSizeInPixels, yy * WorldUtils.TileSizeInPixels, WorldUtils.TileSizeInPixels, WorldUtils.TileSizeInPixels));
				}
			}
		}
	}
}

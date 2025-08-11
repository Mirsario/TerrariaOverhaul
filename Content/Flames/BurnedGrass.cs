using System;
using Microsoft.Xna.Framework;
using Steamworks;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Core.Tags;
using TerrariaOverhaul.Utilities.Terraria;

namespace TerrariaOverhaul.Content.Flames;

public sealed class BurnedGrass : ModTile
{
	private static readonly ContentSet Grass = "Grass";

	public override void SetStaticDefaults()
	{
		Main.tileSolid[Type] = true;
		Main.tileBlockLight[Type] = true;
		Main.tileLighted[Type] = false;
		TileID.Sets.Grass[Type] = true;

		// Merging

		Main.tileMergeDirt[Type] = true;
		Array.Copy(Main.tileMerge[TileID.Grass], Main.tileMerge[Type], Main.tileMerge[TileID.Grass].Length);
		Main.tileMerge[Type][TileID.Grass] = false;
		Main.tileMerge[TileID.Grass][Type] = false;
		Main.tileMerge[Type][TileID.Dirt] = true;
		Main.tileMerge[TileID.Dirt][Type] = true;
		//Main.tileMerge[Type] = Main.tileMerge[TileID.Grass];
		TileID.Sets.ChecksForMerge[Type] = false;

		DustType = 54;
		MinPick = 0;

		RegisterItemDrop(ItemID.DirtBlock);
		AddMapEntry(new Color(32, 32, 32));
	}

	public override void RandomUpdate(int x, int y)
	{
		const int RegrowthRange = 5;
		var basePoint = new Point16(x, y);

		for (int yy = -RegrowthRange; yy <= RegrowthRange; yy++) {
			for (int xx = -RegrowthRange; xx <= RegrowthRange; xx++) {
				var point = basePoint + new Point16(xx, yy);

				if (Main.tile.TryGet(point, out var tile) && tile.HasUnactuatedTile && Grass.HasTile(tile)) {
					WorldGen.ConvertTile(x, y, tile.TileType);
					return;
				}
			}
		}

		WorldGen.ConvertTile(x, y, TileID.Grass);
	}

	public override void KillTile(int x, int y, ref bool fail, ref bool effectOnly, ref bool noItem)
	{
		if (!effectOnly) {
			WorldGen.ConvertTile(x, y, TileID.Dirt, tryBreakTrees: false);
		}
	}
}

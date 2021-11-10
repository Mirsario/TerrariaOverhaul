using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.ObjectData;
using TerrariaOverhaul.Utilities.Extensions;

namespace TerrariaOverhaul.Content.Tiles.Furniture
{
	public class Gramophone : TileBase
	{
		public override void SetStaticDefaults()
		{
			Main.tileFrameImportant[Type] = true;
			Main.tileSolid[Type] = false;
			Main.tileNoAttach[Type] = true;
			Main.tileLavaDeath[Type] = false;

			this.AddTileObjectData(TileObjectData.Style2x2, n => {
				n.Origin = new Point16(0, 1);
				n.LavaDeath = false;
				n.CoordinateWidth = 16;
				n.CoordinateHeights = new[] { 16, 16 };
				n.CoordinatePadding = 2;
				n.UsesCustomCanPlace = true;
				//n.HookPostPlaceMyPlayer = new PlacementHook(ModTileEntityExt.AfterPlacement<GramophoneEntity>, -1, 0, false);
			});

			this.AddMapEntry(Color.Gold, "Gramophone");
		}

		public override void MouseOver(int x, int y) => Main.cursorOverride = 3;
		public override void KillMultiTile(int x, int y, int frameX, int frameY)
		{
			base.KillMultiTile(x, y, frameX, frameY);

			Item.NewItem(x * 16, y * 16, 1, 1, ModContent.ItemType<Items.Placeables.Gramophone>());
		}
		/*public override bool TileFrame(int x, int y, ref bool resetFrame, ref bool noBreak)
		{
			if(!Main.tile.GetUnsafe(x, y, out var tile)) {
				return false;
			}

			short frameX = (short)(tile.frameX % 36);
			short frameY = (short)(tile.frameY % 36);

			if(!TryGetTileEntity<GramophoneEntity>(x, y, out var entity)) {
				return false;
			}

			if(entity.IsActive) {
				frameX += (short)(OverhaulMod.gameUpdateCount / AnimationFrameTime % 2 == 0 ? 36 : 72);
			}
			tile.frameX = frameX;
			tile.frameY = frameY;

			return true;
		}*/
	}
}

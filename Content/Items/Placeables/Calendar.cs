using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace TerrariaOverhaul.Content.Items.Placeables
{
	public class Calendar : ModItem
	{
		public override void SetDefaults()
		{
			//Placeable properties
			item.createTile = ModContent.TileType<Tiles.Furniture.Calendar>();
			//Use properties.
			item.useTime = item.useAnimation = 25;
			item.useStyle = ItemUseStyleID.Swing;
			item.autoReuse = true;
			item.consumable = true;
			//Universal properties.
			item.width = 32;
			item.height = 80;
			item.value = Item.buyPrice(0, 1);
			item.maxStack = 99;
			item.rare = 6;
		}
	}
}

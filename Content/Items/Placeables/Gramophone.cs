using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

namespace TerrariaOverhaul.Content.Items.Placeables
{
	public class Gramophone : ItemBase
	{
		public override void SetDefaults()
		{
			//Placeable properties
			item.createTile = ModContent.TileType<Tiles.Furniture.Gramophone>();
			//Use properties.
			item.useStyle = ItemUseStyleID.Swing;
			item.useTime = item.useAnimation = 25;
			item.autoReuse = true;
			item.consumable = true;
			//Universal properties.
			item.width = 32;
			item.height = 80;
			item.rare = ItemRarityID.LightPurple;
			item.value = Item.buyPrice(0, 5);
			item.maxStack = 99;
			item.noUseGraphic = true;
		}
	}
}

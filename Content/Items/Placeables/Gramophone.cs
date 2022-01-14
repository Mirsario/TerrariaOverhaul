using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace TerrariaOverhaul.Content.Items.Placeables
{
	public class Gramophone : ModItem
	{
		public override void SetDefaults()
		{
			// Placeable properties
			Item.createTile = ModContent.TileType<Tiles.Furniture.Gramophone>();
			// Use properties.
			Item.useStyle = ItemUseStyleID.Swing;
			Item.useTime = Item.useAnimation = 25;
			Item.autoReuse = true;
			Item.consumable = true;
			// Universal properties.
			Item.width = 32;
			Item.height = 80;
			Item.rare = ItemRarityID.LightPurple;
			Item.value = Item.buyPrice(0, 5);
			Item.maxStack = 99;
			Item.noUseGraphic = true;
		}
	}
}

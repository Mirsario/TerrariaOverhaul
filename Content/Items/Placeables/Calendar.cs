using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace TerrariaOverhaul.Content.Items.Placeables;

public class Calendar : ModItem
{
	public override void SetDefaults()
	{
		// Placeable properties
		Item.createTile = ModContent.TileType<Tiles.Furniture.Calendar>();
		// Use properties.
		Item.useTime = Item.useAnimation = 25;
		Item.useStyle = ItemUseStyleID.Swing;
		Item.autoReuse = true;
		Item.consumable = true;
		// Universal properties.
		Item.width = 32;
		Item.height = 80;
		Item.value = Item.buyPrice(0, 1);
		Item.maxStack = 99;
		Item.rare = ItemRarityID.LightPurple;
	}
}

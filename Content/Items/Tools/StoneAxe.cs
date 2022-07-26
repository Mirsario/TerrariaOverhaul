using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Content.Items.Tools;

public class StoneAxe : ModItem
{
	public override void SetDefaults()
	{
		Item.CloneDefaults(ItemID.CopperAxe);

		// Tool properties
		Item.tileBoost = -1;
		// Weapon properties.
		Item.damage /= 2;
		Item.knockBack /= 2f;
		// Use properties.
		Item.useTime = (int)(Item.useTime * 1.2f);
		Item.useAnimation = (int)(Item.useAnimation * 1.2f);
		// Universal properties.
		Item.width = 32;
		Item.height = 32;
		Item.value = Item.sellPrice(0, 0, 0, 5);
	}

	public override void AddRecipes() => this.CreateRecipe(r => r.AddIngredient(ItemID.StoneBlock, 6));
}

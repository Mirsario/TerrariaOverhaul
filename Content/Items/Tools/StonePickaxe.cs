using Terraria;
using Terraria.ID;
using TerrariaOverhaul.Utilities.Extensions;

namespace TerrariaOverhaul.Content.Items.Tools
{
	public class StonePickaxe : ItemBase
	{
		public override void SetDefaults()
		{
			item.CloneDefaults(ItemID.CopperPickaxe);

			//Tool properties
			item.tileBoost = -1;
			//Weapon properties.
			item.damage /= 2;
			item.knockBack /= 2f;
			//Use properties.
			item.useTime = (int)(item.useTime * 1.2f);
			item.useAnimation = (int)(item.useAnimation * 1.2f);
			//Universal properties.
			item.width = 32;
			item.height = 32;
			item.value = Item.sellPrice(0, 0, 0, 5);
		}
		public override void AddRecipes() => this.CreateRecipe(r => r.AddIngredient(ItemID.StoneBlock, 6));
	}
}

using Terraria;
using Terraria.ID;

namespace TerrariaOverhaul.Content.Items.Tools
{
	public class StoneAxe : OverhaulItem
	{
		public override void SetDefaults()
		{
			item.CloneDefaults(ItemID.CopperAxe);

			//Tool properties
			item.tileBoost = -1;
			//Weapon properties.
			item.damage = 2;
			item.knockBack = 0f;
			//Use properties.
			item.useTime = (int)(item.useTime*1.2f);
			item.useAnimation = (int)(item.useAnimation*1.2f);
			//Universal properties.
			item.width = 32;
			item.height = 32;
			item.value = Item.buyPrice(0,0,0,5);
		}
		public override void AddRecipes() => CreateRecipe()
			.AddIngredient(ItemID.StoneBlock,6)
			.Register();
	}
}

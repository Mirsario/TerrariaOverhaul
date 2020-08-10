using System;
using Terraria;
using Terraria.ModLoader;

namespace TerrariaOverhaul.Utilities.Extensions
{
	public static class RecipeExtensions
	{
		public static void CreateRecipe(this ModItem modItem,Action<Recipe> setup)
			=> CreateRecipe(modItem,1,setup);

		public static void CreateRecipe(this ModItem modItem,int amount,Action<Recipe> setup)
		{
			var recipe = modItem.CreateRecipe(amount);

			setup(recipe);

			recipe.Register();
		}
	}
}

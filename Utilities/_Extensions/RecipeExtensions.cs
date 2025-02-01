// Copyright (c) 2020-2025 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using System;
using Terraria;
using Terraria.ModLoader;

namespace TerrariaOverhaul.Utilities;

public static class RecipeExtensions
{
	public static void CreateRecipe(this ModItem modItem, Action<Recipe> setup)
		=> CreateRecipe(modItem, 1, setup);

	public static void CreateRecipe(this ModItem modItem, int amount, Action<Recipe> setup)
	{
		var recipe = modItem.CreateRecipe(amount);

		setup(recipe);

		recipe.Register();
	}
}

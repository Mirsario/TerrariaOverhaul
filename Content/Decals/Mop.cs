// Copyright (c) 2020-2025 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace TerrariaOverhaul.Content.Decals;

internal class Mop : ModItem
{
	public override void SetDefaults()
	{
		// Weapon properties.
		Item.damage = 5;
		Item.knockBack = 5f;
		Item.DamageType = DamageClass.Melee;
		Item.noMelee = true;
		Item.noUseGraphic = true;
		Item.shoot = ModContent.ProjectileType<MopProjectile>();
		Item.shootSpeed = 1f;
		// Use properties.
		Item.useTime = Item.useAnimation = 13;
		Item.useStyle = ItemUseStyleID.Shoot;
		Item.UseSound = SoundID.Item1;
		// Universal properties.
		Item.width = 48;
		Item.height = 48;
		Item.value = Item.sellPrice(0, 0, 0, 5);
	}

	public override bool CanUseItem(Player player)
	{
		return player.ownedProjectileCounts[Item.shoot] < 1;
	}

	public override void AddRecipes() => CreateRecipe()
		.AddIngredient(ItemID.Wood, 5)
		.AddTile(TileID.WorkBenches)
		.Register();
}

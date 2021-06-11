using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Content.Projectiles;
using TerrariaOverhaul.Utilities.Extensions;

namespace TerrariaOverhaul.Content.Items.Tools
{
	public class Mop : ItemBase
	{
		public override void SetDefaults()
		{
			//Weapon properties.
			Item.damage = 5;
			Item.knockBack = 5f;
			Item.DamageType = DamageClass.Melee;
			Item.noMelee = true;
			Item.noUseGraphic = true;
			Item.shoot = ModContent.ProjectileType<MopProjectile>();
			Item.shootSpeed = 1f;
			//Use properties.
			Item.useTime = Item.useAnimation = 13;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.UseSound = SoundID.Item1;
			//Universal properties.
			Item.width = 48;
			Item.height = 48;
			Item.value = Item.sellPrice(0, 0, 0, 5);
		}
		
		public override bool CanUseItem(Player player)
		{
			return player.ownedProjectileCounts[Item.shoot] < 1;
		}
		
		public override void AddRecipes() => this.CreateRecipe(r => {
			r.AddIngredient(ItemID.Wood, 5);
			r.AddTile(TileID.WorkBenches);
		});
	}
}

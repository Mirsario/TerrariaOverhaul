using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.Hooks.Items;
using TerrariaOverhaul.Common.Systems.Camera.ScreenShakes;

namespace TerrariaOverhaul.Common.ModEntities.Items.Overhauls.Generic
{
	public partial class MagicWeapon : AdvancedItem, IShowItemCrosshair
	{
		public static readonly ModSoundStyle MagicBlastSound = new ModSoundStyle($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Items/Magic/MagicBlast", 2, pitchVariance: 0.125f);

		public override ScreenShake OnUseScreenShake => new(4f, 0.2f);

		public override bool ShouldApplyItemOverhaul(Item item)
		{
			//Ignore weapons with non-magic damage types
			if(item.DamageType != DamageClass.Magic && !item.DamageType.CountsAs(DamageClass.Magic)) {
				return false;
			}

			//Avoid tools and placeables
			if(item.pick > 0 || item.axe > 0 || item.hammer > 0 || item.createTile >= TileID.Dirt || item.createWall >= 0) {
				return false;
			}

			//Ignore weapons that don't shoot, don't use mana, or deal hitbox damage 
			if(item.shoot <= ProjectileID.None || item.mana <= 0 || !item.noMelee) {
				return false;
			}

			return true;
		}

		public override void SetDefaults(Item item)
		{
			base.SetDefaults(item);

			if(item.UseSound == SoundID.Item43) {
				item.UseSound = MagicBlastSound;
			}
		}

		public override void HoldItem(Item item, Player player)
		{
			HoldItemCharging(item, player);
		}
		
		public override void ModifyShootStats(Item item, Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
		{
			ShootCharging(item, player, ref position, ref velocity, ref type, ref damage, ref knockback);
		}

		public bool ShowItemCrosshair(Item item, Player player) => true;
	}
}

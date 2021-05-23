using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace TerrariaOverhaul.Common.ModEntities.Items.Overhauls.Generic.Guns
{
	[ItemAttachment(ItemID.Revolver, ItemID.TheUndertaker)]
	public class Revolver : Handgun
	{
		public const float SpinAnimationLengthMultiplier = 2f;
		public const int SpinShotCount = 6;

		//TODO:
		public override bool ShouldApplyItemOverhaul(Item item) => false;

		public override void SetDefaults(Item item)
		{
			base.SetDefaults(item);

			//item.UseSound = new ModSoundStyle(nameof(TerrariaOverhaul), "Assets/Sounds/Items/Guns/Revolver/RevolverFire", 0, volume: 0.15f, pitchVariance: 0.2f);
			PlaySoundOnEveryUse = true;
		}

		public override bool AltFunctionUse(Item item, Player player)
		{
			return true;
		}

		public override float UseTimeMultiplier(Item item, Player player)
		{
			if(player.altFunctionUse == 2) {
				return 1f / (SpinShotCount - 1);
			}

			return base.UseTimeMultiplier(item, player);
		}

		public override float UseSpeedMultiplier(Item item, Player player)
		{
			if(player.altFunctionUse == 2) {
				return 0.6f;
			}

			return base.UseTimeMultiplier(item, player);
		}

		public override void ModifyShootStats(Item item, Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
		{
			base.ModifyShootStats(item, player, ref position, ref velocity, ref type, ref damage, ref knockback);

			if(player.altFunctionUse == 2) {
				velocity = velocity.RotatedByRandom(MathHelper.ToRadians(6f));
				damage = (int)(damage * 0.75f);
			}
		}

		public override void UseAnimation(Item item, Player player)
		{
			/*if(player.altFunctionUse == 2) {
				player.reuseDelay = System.Math.Max(player.reuseDelay, item.useAnimation * 150);
			}*/

			base.UseAnimation(item, player);
		}
	}
}

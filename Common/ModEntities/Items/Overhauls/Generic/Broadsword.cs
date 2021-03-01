using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.Systems.Camera.ScreenShakes;
using TerrariaOverhaul.Utilities.DataStructures;

namespace TerrariaOverhaul.Common.ModEntities.Items.Overhauls.Generic
{
	public class Broadsword : MeleeWeapon
	{
		public override bool ShouldApplyItemOverhaul(Item item)
		{
			//Broadswords always swing, deal melee damage, don't have channeling, and are visible
			if(item.useStyle != ItemUseStyleID.Swing || item.noMelee || item.channel || item.noUseGraphic) {
				return false;
			}

			//Avoid tools and blocks
			if(item.pick > 0 || item.axe > 0 || item.hammer > 0 || item.createTile >= TileID.Dirt || item.createWall >= 0) {
				return false;
			}

			if(item.DamageType != DamageClass.Melee) {
				return false;
			}

			return true;
		}
		public override void SetDefaults(Item item)
		{
			base.SetDefaults(item);

			//item.useAnimation /= 2;
			//item.useTime /= 2;
			//item.reuseDelay += item.useAnimation;
		}
		public override void UseAnimation(Item item, Player player)
		{
			base.UseAnimation(item, player);

			FlippedAttack = AttackNumber % 2 != 0;

			var verticalVelocityScaleGradient = new Gradient<float>(
				(-1.0f, 3.0f),
				(-0.5f, 3.0f),
				( 0.0f, 3.0f),
				( 0.5f, 9.0f),
				( 1.0f, 9.0f)
			);
			var dashVelocity = new Vector2(
				3f * (player.velocity.Y == 0f ? 1f : 0.5f),
				verticalVelocityScaleGradient.GetValue(AttackDirection.Y)
			);

			BasicVelocityDash(player, AttackDirection, dashVelocity, false);

			if(!Main.dedServ) {
				ScreenShakeSystem.New(3f, item.useAnimation / 120f);
			}
		}
		public override bool ShouldBeAttacking(Item item, Player player)
		{
			return base.ShouldBeAttacking(item, player) && player.itemAnimation >= player.itemAnimationMax / 2;
		}
	}
}

using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.Systems.Camera.ScreenShakes;
using TerrariaOverhaul.Common.Systems.Time;
using TerrariaOverhaul.Content.Gores;
using TerrariaOverhaul.Utilities;
using TerrariaOverhaul.Utilities.Extensions;

namespace TerrariaOverhaul.Common.ModEntities.Items.Overhauls.Generic.Guns
{
	public class Minigun : Gun
	{
		private float speedFactor;

		public virtual float MinSpeedFactor => 0.333f;
		public virtual float AccelerationTime => 1f;
		public virtual float DecelerationTime => 1f;
		public virtual bool DoSpawnCasings => true;

		public override float OnUseVisualRecoil => 5f;
		public override ScreenShake OnUseScreenShake => new(5f, 0.25f);

		public override bool ShouldApplyItemOverhaul(Item item)
		{
			//Miniguns always use bullets.
			if(item.useAmmo != AmmoID.Bullet) {
				return false;
			}

			if(item.UseSound != SoundID.Item11 && item.UseSound != SoundID.Item40 && item.UseSound != SoundID.Item41) {
				return false;
			}

			//Exclude slow firing guns.
			if(item.useTime >= 10) {
				return false;
			}

			return true;
		}
		
		public override void SetDefaults(Item item)
		{
			base.SetDefaults(item);

			item.UseSound = new ModSoundStyle(nameof(TerrariaOverhaul), "Assets/Sounds/Items/Guns/Minigun/MinigunFire", 0, volume: 0.15f, pitchVariance: 0.2f);
			speedFactor = MinSpeedFactor;
			PlaySoundOnEveryUse = true;
		}

		public override float UseSpeedMultiplier(Item item, Player player)
			=> base.UseSpeedMultiplier(item, player) * speedFactor;

		public override void ModifyWeaponDamage(Item item, Player player, ref StatModifier damage, ref float flat)
		{
			base.ModifyWeaponDamage(item, player, ref damage, ref flat);
		}

		public override void HoldItem(Item item, Player player)
		{
			base.HoldItem(item, player);

			if(player.controlUseItem) {
				speedFactor = MathUtils.StepTowards(speedFactor, 1f, AccelerationTime * TimeSystem.LogicDeltaTime);
			} else {
				speedFactor = MinSpeedFactor; //speedFactor = MathUtils.StepTowards(speedFactor, MinSpeedFactor, DecelerationTime * TimeSystem.LogicDeltaTime);
			}
		}

		public override bool? UseItem(Item item, Player player)
		{
			if(!Main.dedServ && DoSpawnCasings) {
				int numCasings = player.altFunctionUse == 2 ? 2 : 1;
				
				for(int i = 0; i < numCasings; i++) {
					SpawnCasings<BulletCasing>(player);
				}
			}

			ApplyVelocityRecoil(item, player);

			return base.UseItem(item, player);
		}

		private static void ApplyVelocityRecoil(Item item, Player player)
		{
			var mouseWorld = player.GetModPlayer<Players.PlayerDirectioning>().mouseWorld;
			var direction = (player.Center - mouseWorld).SafeNormalize(default);
			var modifiedDirection = new Vector2(direction.X, direction.Y * Math.Abs(direction.Y));
			var velocity = modifiedDirection * new Vector2(item.useTime / 15f, item.useTime / 2.875f);

			//Disable horizontal velocity recoil whenever the player is holding a directional key opposite to the direction of the dash.
			if(player.KeyDirection() == -Math.Sign(velocity.X)) {
				velocity.X = 0f;
			}

			//Disable vertical velocity whenever aiming upwards or standing on the ground
			if(velocity.Y > 0f || player.velocity.Y == 0f) {
				velocity.Y = 0f;
			}

			player.AddLimitedVelocity(velocity, new Vector2(3f, 5f));
		}
	}
}

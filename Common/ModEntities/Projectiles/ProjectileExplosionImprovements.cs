using Microsoft.Xna.Framework;
using System;
using Terraria;
using TerrariaOverhaul.Common.Systems.Camera.ScreenShakes;
using TerrariaOverhaul.Common.Systems.Decals;
using TerrariaOverhaul.Common.Tags;
using TerrariaOverhaul.Utilities;
using TerrariaOverhaul.Utilities.Extensions;

namespace TerrariaOverhaul.Common.ModEntities.Projectiles
{
	public class ProjectileExplosionImprovements : GlobalProjectileBase
	{
		private Vector2 maxSize;

		public override bool InstancePerEntity => true;

		public override bool InstanceForEntity(Projectile projectile, bool lateInstantiation)
			=> OverhaulProjectileTags.Explosive.Has(projectile.type);

		public override bool PreAI(Projectile projectile)
		{
			maxSize = Vector2.Max(maxSize, projectile.Size);

			return true;
		}

		public override void Kill(Projectile projectile, int timeLeft)
		{
			maxSize = Vector2.Max(maxSize, projectile.Size);

			if(maxSize.X <= 0f || maxSize.Y <= 0f) {
				return;
			}

			float maxPower = (float)Math.Sqrt(maxSize.X * maxSize.Y);
			float knockbackRange = maxPower;
			float knockbackRangeSquared = knockbackRange * knockbackRange;
			var center = projectile.Center;

			//Knockback
			for(int i = 0; i < Main.maxPlayers + Main.maxItems + Main.maxGore; i++) {
				ref Vector2 velocity = ref projectile.velocity; //Unused assignment.
				Rectangle rectangle;
				object entity;

				if(i < Main.maxPlayers) {
					var player = Main.player[i];

					if(player?.active != true) {
						continue;
					}

					entity = player;
					velocity = ref player.velocity;
					rectangle = player.getRect();
				} else if(i < Main.maxPlayers + Main.maxItems) {
					var item = Main.item[i - Main.maxPlayers];

					if(item?.active != true) {
						continue;
					}

					entity = item;
					velocity = ref item.velocity;
					rectangle = item.getRect();
				} else {
					var gore = Main.gore[i - Main.maxPlayers - Main.maxItems];

					if(gore?.active != true) {
						continue;
					}

					entity = gore;
					velocity = ref gore.velocity;
					rectangle = gore.AABBRectangle;
				}

				float sqrDistance = Vector2.DistanceSquared(rectangle.GetCorner(center), center);

				if(sqrDistance >= knockbackRangeSquared) {
					continue;
				}

				var direction = (rectangle.Center() - center).SafeNormalize(default);
				float distance = (float)Math.Sqrt(sqrDistance);

				if(float.IsNaN(distance) || direction == default) {
					continue;
				}

				//Explosions have a chance to set gore on fire.
				if(entity is Systems.Gores.OverhaulGore goreExt && Main.rand.Next(5) == 0) {
					goreExt.onFire = true;
				}

				velocity += direction * MathUtils.DistancePower(distance, knockbackRange) * maxPower / 13f;

				if(velocity.HasNaNs()) {
					velocity = Vector2.Zero;
				}
			}

			if(!Main.dedServ) {
				//Screenshake
				if(Main.LocalPlayer != null) {
					float distance = Vector2.Distance(Main.LocalPlayer.Center, center);
					float screenshakeRange = maxPower * 10f;
					float power = MathUtils.DistancePower(distance, screenshakeRange) * 15f;

					if(power > 0f) {
						ScreenShakeSystem.New(power, 0.5f);
					}
				}

				//Decal
				var rect = new Rectangle((int)projectile.Center.X, (int)projectile.Center.Y, 0, 0);

				rect.Inflate(64, 64);

				DecalSystem.AddDecals(Mod.GetTexture("Assets/Sprites/ExplosionDecal").Value, rect, null, new Color(255, 255, 255, 128));
			}
		}
	}
}

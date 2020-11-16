using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using TerrariaOverhaul.Common.Systems.Camera.ScreenShakes;
using TerrariaOverhaul.Common.Tags;
using TerrariaOverhaul.Utilities;
using TerrariaOverhaul.Utilities.Extensions;

namespace TerrariaOverhaul.Common.ModEntities.Projectiles
{
	//TODO: Use conditional instancing when it's implemented for projectiles.
	public class ProjectileExplosionImprovements : GlobalProjectileBase
	{
		public override void Kill(Projectile projectile, int timeLeft)
		{
			if(!OverhaulProjectileTags.Explosive.Has(projectile.type)) {
				return;
			}

			float maxPower = projectile.width * projectile.height;
			float knockbackRange = maxPower / 5f;
			float knockbackRangeSquared = knockbackRange * knockbackRange;
			var center = projectile.Center;

			//Knockback
			for(int i = 0; i < Main.maxPlayers + Main.maxItems + Main.maxGore; i++) {
				ref Vector2 velocity = ref projectile.velocity; //Unused assignment.
				Rectangle rectangle;

				if(i < Main.maxPlayers) {
					var player = Main.player[i];

					if(player?.active != true) {
						continue;
					}

					velocity = ref player.velocity;
					rectangle = player.getRect();
				} else if(i < Main.maxPlayers + Main.maxItems) {
					var item = Main.item[i - Main.maxPlayers];

					if(item?.active != true) {
						continue;
					}

					velocity = ref item.velocity;
					rectangle = item.getRect();
				} else {
					var gore = Main.gore[i - Main.maxPlayers - Main.maxItems];

					if(gore?.active != true) {
						continue;
					}

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

				velocity += direction * MathUtils.DistancePower(distance, knockbackRange) * maxPower / 50f;

				if(velocity.HasNaNs()) {
					velocity = Vector2.Zero;
				}
			}

			//Screenshake
			if(!Main.dedServ && Main.LocalPlayer != null) {
				float distance = Vector2.Distance(Main.LocalPlayer.Center, center);
				float screenshakeRange = maxPower;
				float power = MathUtils.DistancePower(distance, screenshakeRange) * 15f;

				if(power > 0f) {
					ScreenShakeSystem.New(power, 0.5f);
				}
			}
		}
	}
}

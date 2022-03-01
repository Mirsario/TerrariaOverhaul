﻿using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.Tags;

namespace TerrariaOverhaul.Common.BloodAndGore
{
	[Autoload(Side = ModSide.Client)]
	public class ProjectileGoreInteraction : GlobalProjectile
	{
		public bool DontHitGore { get; set; }

		public override bool InstancePerEntity => true;

		public override void AI(Projectile projectile)
		{
			// Reset dontHitGore every X ticks when the projectile's flying somewhere
			if (DontHitGore && projectile.position != projectile.oldPosition && projectile.timeLeft % 3 == 0) {
				DontHitGore = false;
			}

			// Skip gore enumeration when there's nothing to do.
			if (DontHitGore) {
				return;
			}

			bool incendiary = OverhaulProjectileTags.Incendiary.Has(projectile.type);
			bool extinguisher = !incendiary && OverhaulProjectileTags.Extinguisher.Has(projectile.type);

			for (int i = 0; i < Main.maxGore; i++) {
				var gore = Main.gore[i];

				if (gore == null || !gore.active || Main.gore[i] is not OverhaulGore goreExt) {
					continue;
				}

				// For now, only bleeding gores are considered hittable.
				if (!goreExt.bleedColor.HasValue) {
					continue;
				}

				// Intersection check
				if (!projectile.getRect().Intersects(new Rectangle((int)gore.position.X, (int)gore.position.Y, (int)goreExt.Width, (int)goreExt.Height))) {
					continue;
				}

				// Interact
				float hitPower = 1f;

				if (incendiary) {
					goreExt.onFire = true;

					continue;
				} else if (extinguisher) {
					goreExt.onFire = false;

					hitPower = 5f;
				}

				DontHitGore = goreExt.HitGore(projectile.velocity.SafeNormalize(default), hitPower);

				break;
			}
		}
	}
}

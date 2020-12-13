using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Content.SimpleEntities;
using TerrariaOverhaul.Core.Systems.SimpleEntities;

namespace TerrariaOverhaul.Common.ModEntities.Projectiles
{
	[Autoload(Side = ModSide.Client)]
	public sealed class ProjectileSlimeGunParticles : GlobalProjectileBase
	{
		public override void AI(Projectile projectile)
		{
			if(projectile.type == ProjectileID.SlimeGun) {
				SimpleEntity.Instantiate<BloodParticle>(p => {
					p.position = projectile.Center;
					p.velocity = projectile.velocity * 60f + Main.rand.NextVector2Circular(20f, 20f);
					p.color = new Color(0, 80, 255, 100);
				});
			}
		}
	}
}

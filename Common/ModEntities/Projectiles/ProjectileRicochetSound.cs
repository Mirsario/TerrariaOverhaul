using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.Tags;

namespace TerrariaOverhaul.Common.ModEntities.Projectiles
{
	//TODO: Use conditional instancing when it's implemented for projectiles.
	public class ProjectileRicochetSound : GlobalProjectileBase
	{
		public static readonly ModSoundStyle RicochetSound = new ModSoundStyle(nameof(TerrariaOverhaul), "Assets/Sounds/HitEffects/Ricochet", 2, volume: 0.1f); 

		public override bool InstancePerEntity => true;

		public override bool OnTileCollide(Projectile projectile, Vector2 oldVelocity)
		{
			if(!Main.dedServ && OverhaulProjectileTags.Bullet.Has(projectile.type)) {
				SoundEngine.PlaySound(RicochetSound, projectile.Center);
			}

			return true;
		}
	}
}

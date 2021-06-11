using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.Systems.Decals;
using TerrariaOverhaul.Common.Tags;

namespace TerrariaOverhaul.Common.ModEntities.Projectiles
{
	//TODO: Use conditional instancing when it's implemented for projectiles.
	[Autoload(Side = ModSide.Client)]
	public class ProjectileIncendiaryDecals : GlobalProjectileBase
	{
		public override bool InstancePerEntity => true;

		public override void Kill(Projectile projectile, int timeLeft)
		{
			if(!OverhaulProjectileTags.Incendiary.Has(projectile.type)) {
				return;
			}

			AddDecals(projectile, 32, 0.2f);
		}
		
		public override void PostAI(Projectile projectile)
		{
			if(!OverhaulProjectileTags.Incendiary.Has(projectile.type) || Main.GameUpdateCount % 2 != 0) {
				return;
			}

			AddDecals(projectile, 32, 0.015f);
		}

		private void AddDecals(Projectile projectile, int size, float alpha)
		{
			var rect = new Rectangle((int)projectile.Center.X, (int)projectile.Center.Y, 0, 0);

			rect.Inflate(size, size);

			DecalSystem.AddDecals(Mod.GetTexture("Assets/Textures/ExplosionDecal").Value, rect, new Color(255, 255, 255, (byte)(alpha * 255f)));
		}
	}
}

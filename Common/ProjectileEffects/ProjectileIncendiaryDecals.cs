using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.Decals;
using TerrariaOverhaul.Common.Tags;

namespace TerrariaOverhaul.Common.ProjectileEffects;

//TODO: Use conditional instancing when it's implemented for projectiles.
[Autoload(Side = ModSide.Client)]
public class ProjectileIncendiaryDecals : GlobalProjectile
{
	private static Asset<Texture2D>? explosionDecal;

	public override bool InstancePerEntity => true;

	public override void Load()
	{
		explosionDecal = Mod.Assets.Request<Texture2D>("Assets/Textures/ExplosionDecal");
	}

	public override void Kill(Projectile projectile, int timeLeft)
	{
		if (!OverhaulProjectileTags.Incendiary.Has(projectile.type)) {
			return;
		}

		AddDecals(projectile, 32, 0.2f);
	}

	public override void PostAI(Projectile projectile)
	{
		if (!OverhaulProjectileTags.Incendiary.Has(projectile.type) || Main.GameUpdateCount % 2 != 0) {
			return;
		}

		AddDecals(projectile, 32, 0.015f);
	}

	private static void AddDecals(Projectile projectile, int size, float alpha)
	{
		if (explosionDecal?.Value is not Texture2D decalTexture) {
			return;
		}

		var rect = new Rectangle((int)projectile.Center.X, (int)projectile.Center.Y, 0, 0);

		rect.Inflate(size, size);

		DecalSystem.AddDecals(decalTexture, rect, new Color(255, 255, 255, (byte)(alpha * 255f)));
	}
}

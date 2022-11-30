using Terraria;
using TerrariaOverhaul.Common.BloodAndGore;
using TerrariaOverhaul.Common.Decals;

namespace TerrariaOverhaul.Content.Projectiles;

public class MopProjectile : SpearProjectileBase
{
	protected override float HoldoutRangeMin => 40f;
	protected override float HoldoutRangeMax => 80f;

	public override void SetDefaults()
	{
		base.SetDefaults();

		Projectile.width = 16;
		Projectile.height = 16;

		if (!Main.dedServ && Projectile.TryGetGlobalProjectile(out ProjectileGoreInteraction goreInteraction)) {
			goreInteraction.FireInteraction = ProjectileGoreInteraction.FireProperties.Extinguisher;
			goreInteraction.HitEffectMultiplier = 0.333f; // Produce less explosive blood when destroying gore.
			goreInteraction.DisableGoreHitAudio = false; // Hit gore silently.
			goreInteraction.DisableGoreHitCooldown = true; // Hit much more gore than usually.
		}
	}

	public override void PostAI()
	{
		base.PostAI();

		if (!Main.dedServ) {
			DecalSystem.ClearDecals(Projectile.getRect());
		}
	}
}

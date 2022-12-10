using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.Tags;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.BloodAndGore;

[Autoload(Side = ModSide.Client)]
public class ProjectileGoreInteraction : GlobalProjectile
{
	public enum FireProperties : byte
	{
		None,
		Incendiary,
		Extinguisher
	}

	private bool dontHitGore;

	public float HitPower { get; set; } = 1f;
	public float HitEffectMultiplier { get; set; } = 1f;
	public bool DisableGoreHitCooldown { get; set; }
	public bool DisableGoreHitAudio { get; set; }
	public FireProperties FireInteraction { get; set; }

	public override bool InstancePerEntity => true;

	public override void OnSpawn(Projectile projectile, IEntitySource source)
	{
		base.OnSpawn(projectile, source);

		if (OverhaulProjectileTags.Incendiary.Has(projectile.type)) {
			FireInteraction = FireProperties.Incendiary;
		} else if (OverhaulProjectileTags.Extinguisher.Has(projectile.type)) {
			FireInteraction = FireProperties.Extinguisher;
		}
	}

	public override bool PreAI(Projectile projectile)
	{
		// Reset dontHitGore every X ticks when the projectile's flying somewhere
		if (dontHitGore && projectile.position != projectile.oldPosition && projectile.timeLeft % 3 == 0) {
			dontHitGore = false;
		}

		// Skip gore enumeration when there's nothing to do.
		if (dontHitGore) {
			return true;
		}

		for (int i = 0; i < Main.maxGore; i++) {
			var gore = Main.gore[i];

			if (gore == null || !gore.active || Main.gore[i] is not OverhaulGore goreExt) {
				continue;
			}

			// Intersection check
			if (!projectile.getRect().Intersects(new Rectangle((int)gore.position.X, (int)gore.position.Y, (int)goreExt.Width, (int)goreExt.Height))) {
				continue;
			}

			// Interact
			if (FireInteraction == FireProperties.Extinguisher) {
				goreExt.OnFire = false;
			} else if (FireInteraction == FireProperties.Incendiary) {
				goreExt.OnFire = true;

				continue;
			}

			bool isFlesh = goreExt.BleedColor.HasValue;
			float hitPower = HitPower;
			float damageScale = hitPower;
			float velocityScale = hitPower;

			if (!isFlesh) {
				damageScale = 0f;
			}

			goreExt.ApplyForce(projectile.velocity.SafeNormalize(-Vector2.UnitY) * velocityScale);

			bool hasHitGore = goreExt.Damage(damageScale, HitEffectMultiplier, DisableGoreHitAudio);

			if (hasHitGore && !DisableGoreHitCooldown) {
				dontHitGore = true;
				break;
			}
		}

		return true;
	}
}

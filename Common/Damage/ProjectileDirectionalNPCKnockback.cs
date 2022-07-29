using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace TerrariaOverhaul.Common.Damage;

public sealed class ProjectileDirectionalNPCKnockback : GlobalProjectile
{
	public override void ModifyHitNPC(Projectile projectile, NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
	{
		if (target.TryGetGlobalNPC(out NPCDirectionalKnockback npcKnockback)) {
			Vector2 projectileVelocity = projectile.oldVelocity != Vector2.Zero ? projectile.oldVelocity : projectile.velocity;
			Vector2 direction = projectileVelocity.SafeNormalize(Vector2.UnitX * hitDirection);

			npcKnockback.SetNextKnockbackDirection(direction);
		}
	}
}

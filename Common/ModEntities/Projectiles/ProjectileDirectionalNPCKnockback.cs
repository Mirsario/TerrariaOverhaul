using Microsoft.Xna.Framework;
using Terraria;
using TerrariaOverhaul.Common.ModEntities.NPCs;

namespace TerrariaOverhaul.Common.ModEntities.Projectiles
{
	public sealed class ProjectileDirectionalNPCKnockback : GlobalProjectileBase
	{
		public override void ModifyHitNPC(Projectile projectile, NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
		{
			if(target.TryGetGlobalNPC(out NPCDirectionalKnockback npcKnockback)) {
				Vector2 direction = projectile.velocity.SafeNormalize(Vector2.UnitX * hitDirection);

				npcKnockback.SetNextKnockbackDirection(direction);
			}
		}
	}
}

// Copyright (c) 2020-2026 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace TerrariaOverhaul.Common.Damage;

internal sealed class ProjectileDirectionalNPCKnockback : GlobalProjectile
{
	public override void ModifyHitNPC(Projectile projectile, NPC target, ref NPC.HitModifiers modifiers)
	{
		if (target.TryGetGlobalNPC(out NPCDirectionalKnockback npcKnockback)) {
			Vector2 projectileVelocity = projectile.oldVelocity != Vector2.Zero ? projectile.oldVelocity : projectile.velocity;
			Vector2 direction = projectileVelocity.SafeNormalize(Vector2.UnitX * modifiers.HitDirection);

			npcKnockback.SetNextKnockbackDirection(direction);
		}
	}
}

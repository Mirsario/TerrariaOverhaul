using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace TerrariaOverhaul.Common.Damage;

public sealed class ItemNpcStuns : GlobalItem
{
	public override void OnHitNPC(Item item, Player player, NPC target, int damage, float knockBack, bool crit)
	{
		const int MinStunTime = 1;
		const int MaxStunTime = 60;
		//const float PowerAttackStunMultiplier = 1.5f;

		if (target.TryGetGlobalNPC(out NPCAttackCooldowns cooldowns)) {
			uint swingTime = (uint)Math.Max(0, player.itemAnimationMax);
			uint cooldownTicks = (uint)MathHelper.Clamp(swingTime, MinStunTime, MaxStunTime);

			// Power attacks have increased stun time
			//if (item.TryGetGlobalItem(out ItemPowerAttacks powerAttacks) && powerAttacks.PowerAttack) {
			//	cooldownTicks = (uint)(cooldownTicks * PowerAttackStunMultiplier);
			//}

			bool isDamage = damage > 0;

			cooldowns.SetAttackCooldown(target, cooldownTicks, isDamage);
		}
	}
}

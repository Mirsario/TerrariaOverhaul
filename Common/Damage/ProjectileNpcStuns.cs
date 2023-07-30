using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace TerrariaOverhaul.Common.Damage;

public sealed class ProjectileNpcStuns : GlobalProjectile
{
	private uint cooldownTicks = 0;

	public override bool InstancePerEntity => true;

	public override void OnSpawn(Projectile projectile, IEntitySource source)
	{
		const int MinStunTime = 1;
		const int MaxStunTime = 20;

		if (source is EntitySource_ItemUse { Entity: Player player } itemUse) {
			int baseTime;

			if (projectile.penetrate > 0 || projectile.aiStyle == ProjAIStyleID.Spear) {
				baseTime = player.itemAnimationMax;
			} else if (projectile.usesLocalNPCImmunity) {
				baseTime = projectile.localNPCHitCooldown;
			} else if (projectile.usesIDStaticNPCImmunity) {
				baseTime = projectile.idStaticNPCHitCooldown;
			} else {
				baseTime = 0;
			}
			
			cooldownTicks = (uint)MathHelper.Clamp(baseTime, MinStunTime, MaxStunTime);
			//Main.NewText($"Projectile StunTicks: {cooldownTicks}");
		}
	}

	public override void OnHitNPC(Projectile projectile, NPC target, int damage, float knockback, bool crit)
	{
		if (cooldownTicks != 0 && target.TryGetGlobalNPC(out NPCAttackCooldowns cooldowns)) {
			bool isDamage = damage > 0;

			cooldowns.SetAttackCooldown(target, cooldownTicks, isDamage);
		}
	}
}

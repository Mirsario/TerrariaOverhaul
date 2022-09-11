using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace TerrariaOverhaul.Common.Damage;

public class NPCAttackCooldowns : GlobalNPC
{
	private static int DefaultEnemyAttackCooldown => 5;
	private static int DefaultBossAttackCooldown => 5;

	private Color? defaultColor;

	public int AttackCooldown { get; private set; }
	public bool ShowDamagedEffect { get; private set; }

	public override bool InstancePerEntity => true;

	public override bool PreAI(NPC npc)
	{
		if (AttackCooldown > 0) {
			AttackCooldown--;

			if (AttackCooldown == 0 && ShowDamagedEffect) {
				if (defaultColor.HasValue) {
					npc.color = defaultColor.Value;
				}

				ShowDamagedEffect = false;
			}
		}

		return true;
	}

	public override bool? CanHitNPC(NPC npc, NPC target)
		=> AttackCooldown > 0 ? false : null;

	public override bool CanHitPlayer(NPC npc, Player target, ref int cooldownSlot)
		=> AttackCooldown <= 0;

	public override void OnHitByItem(NPC npc, Player player, Item item, int damage, float knockback, bool crit)
		=> ApplyDefaultCooldown(npc);

	public override void OnHitByProjectile(NPC npc, Projectile projectile, int damage, float knockback, bool crit)
		=> ApplyDefaultCooldown(npc);

	public void SetAttackCooldown(NPC npc, int ticks, bool isDamage)
	{
		if (isDamage && !ShowDamagedEffect) {
			bool isBoss = npc.boss || NPCID.Sets.ShouldBeCountedAsBoss[npc.type];
			float blend = isBoss ? 0.33f : 0.5f;

			defaultColor = npc.color;
			npc.color = Color.Lerp(npc.color, new Color(0.9f, 0.0f, 0.0f), blend);
			ShowDamagedEffect |= isDamage;
		}

		AttackCooldown = Math.Max(ticks, AttackCooldown);
	}

	private void ApplyDefaultCooldown(NPC npc)
	{
		bool isBoss = npc.boss || NPCID.Sets.ShouldBeCountedAsBoss[npc.type];
		int cooldown = isBoss ? DefaultBossAttackCooldown : DefaultEnemyAttackCooldown;

		SetAttackCooldown(npc, cooldown, true);
	}
}

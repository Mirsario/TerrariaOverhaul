using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.Camera;
using TerrariaOverhaul.Utilities;

#pragma warning disable IDE0060 // Remove unused parameter

namespace TerrariaOverhaul.Common.Damage;

[Autoload(Side = ModSide.Client)]
public class NPCHitScreenShake : GlobalNPC
{
	public ScreenShake? OnHitShake { get; set; }
	public ScreenShake? OnDeathShake { get; set; }

	public override bool InstancePerEntity => true;

	public override void SetDefaults(NPC npc)
	{
		const float BaseShakeRange = 256f * 5;

		if (npc.boss || NPCID.Sets.ShouldBeCountedAsBoss[npc.type]) {
			OnHitShake = new(0.50f, 0.30f, range: BaseShakeRange, uniqueId: "BossHit");
			OnDeathShake = new(1.00f, 1.00f, range: BaseShakeRange * 3f, uniqueId: "BossDeath");
		} else {
			OnHitShake = new(0.17f, 0.15f, range: BaseShakeRange, uniqueId: "NpcHit");
			OnDeathShake = new(0.30f, 0.30f, range: BaseShakeRange, uniqueId: "NpcDeath");
		}
	}

	public override void OnHitByItem(NPC npc, Player player, Item item, int damage, float knockback, bool crit)
		=> OnHit(npc, damage, knockback, crit);

	public override void OnHitByProjectile(NPC npc, Projectile projectile, int damage, float knockback, bool crit)
		=> OnHit(npc, damage, knockback, crit);

	private void OnHit(NPC npc, int damage, float knockback, bool crit)
	{
		bool isDead = npc.life <= 0;

		if ((isDead ? OnDeathShake : OnHitShake) is ScreenShake shake) {
			shake.Position = npc.Center;
			shake.Power *= MathUtils.Clamp01(damage / 10f);
			shake.UniqueId ??= isDead ? "CustomNpcDeath" : "CustomNpcHit";

			ScreenShakeSystem.New(shake);
		}
	}
}

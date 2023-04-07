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
		const float BaseShakeRange = 256f * 5f;

		if (npc.boss || NPCID.Sets.ShouldBeCountedAsBoss[npc.type]) {
			OnHitShake = new(0.25f, 0.30f) {
				Range = BaseShakeRange,
				UniqueId = "BossHit",
			};
			OnDeathShake = new(1.00f, 1.00f) {
				Range = BaseShakeRange * 3f,
				UniqueId = "BossDeath",
			};
		} else {
			OnHitShake = new(0.17f, 0.15f) {
				Range = BaseShakeRange,
				UniqueId = "NpcHit",
			};
			OnDeathShake = new(0.30f, 0.30f) {
				Range = BaseShakeRange,
				UniqueId = "NpcDeath",
			};
		}
	}

	public override void OnHitByItem(NPC npc, Player player, Item item, NPC.HitInfo hit, int damageDone)
		=> OnHit(npc, hit, damageDone);

	public override void OnHitByProjectile(NPC npc, Projectile projectile, NPC.HitInfo hit, int damageDone)
		=> OnHit(npc, hit, damageDone);

	private void OnHit(NPC npc, NPC.HitInfo hit, int damageDone)
	{
		bool isDead = npc.life <= 0;

		if ((isDead ? OnDeathShake : OnHitShake) is ScreenShake shake) {
			shake.Power *= MathUtils.Clamp01(hit.Damage * 0.1f);
			shake.UniqueId ??= isDead ? "CustomNpcDeath" : "CustomNpcHit";

			ScreenShakeSystem.New(shake, npc.Center);
		}
	}
}

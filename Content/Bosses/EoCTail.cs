using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace TerrariaOverhaul.Content.Bosses;

public class EoCTail : ModNPC
{
	public override void SetStaticDefaults()
	{
		DisplayName.SetDefault("Eye of Cthulhu");
	}

	public override void SetDefaults()
	{
		// Copying some stuff from vanilla EoC for convinience
		NPC.CloneDefaults(NPCID.EyeofCthulhu);
		NPC.width = NPC.height = 32;
		NPC.aiStyle = -1;

		// Combat
		SegmentDistance = 5;
		defaultDamage = NPC.damage;
	}

	private int defaultDamage;

	public int freezeTime;
	public bool shouldFreeze;

	private int MissCounter {
		get => (int)Parent.ai[3];
		set => Parent.ai[3] = value;
	}

	public bool hasHitAtLeastOne;

	private float SegmentDistance {
		get => NPC.ai[0];
		set => NPC.ai[0] = value;
	}

	private bool IsExtending => NPC.ai[1] > 0;

	public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position) => false;

	public NPC ParentSegment {
		get {
			if (NPC.ai[3] >= 0 && NPC.ai[3] < Main.maxNPCs) {
				return Main.npc[(int)NPC.ai[3]];
			}

			return null;
		}
	}

	public NPC Parent {
		get {
			if (NPC.realLife >= 0 && NPC.realLife < Main.maxNPCs) {
				return Main.npc[NPC.realLife];
			}

			return null;
		}
	}

	public override void AI()
	{
		if (ParentSegment == null || !ParentSegment.active || Parent == null || !Parent.active) {
			NPC.life = 0;
			NPC.active = false;
			return;
		}

		if (freezeTime > 0) {
			freezeTime--;
			return;
		}

		SegmentDistance -= 0.5f;

		if (IsExtending) {
			if (NPC.ai[1] == EoCRework.WhipAttackTime) {
				NPC.damage = defaultDamage;
				hasHitAtLeastOne = false;
			}

			if (MissCounter >= 3 && shouldFreeze && SegmentDistance >= 32) {
				freezeTime = 640;
				shouldFreeze = false;
				NPC.damage = 0;
				hasHitAtLeastOne = true;
			}

			if (--NPC.ai[1] <= 0 && !hasHitAtLeastOne && ParentSegment == Parent) {
				MissCounter++;
			}

			SegmentDistance += 1f;
		}

		NPC.Center = ParentSegment.Center - new Vector2(SegmentDistance, 0).RotatedBy(ParentSegment.rotation);

		NPC.rotation = Utils.AngleLerp(NPC.rotation, (ParentSegment.Center - NPC.Center).ToRotation(), 0.01f * SegmentDistance);

		SegmentDistance = MathHelper.Clamp(SegmentDistance, 8f, 32f);
	}

	public override void ModifyHitPlayer(Player target, ref int damage, ref bool crit)
	{
		hasHitAtLeastOne = true;
	}

	public override void ModifyHitByItem(Player player, Item item, ref int damage, ref float knockback, ref bool crit)
	{
		damage = (int)(damage * 5.0f);
	}

	public override void ModifyHitByProjectile(Projectile projectile, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
	{
		damage = (int)(damage * 5.0f);
	}
}

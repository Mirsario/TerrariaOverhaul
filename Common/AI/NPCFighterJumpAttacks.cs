using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Core.Configuration;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.AI;

// This makes fighter AIs jump at their targets when they get close enough.
public class NPCFighterJumpAttacks : GlobalNPC
{
	public static readonly ConfigEntry<bool> EnableEnemyLunges = new(ConfigSide.Both, "Enemies", nameof(EnableEnemyLunges), () => true);

	private float prevDistance;

	public override bool InstancePerEntity => true;

	public override bool AppliesToEntity(NPC npc, bool lateInstantiation)
		=> lateInstantiation && npc.aiStyle == NPCAIStyleID.Fighter;

	public override void AI(NPC npc)
	{
		if (!EnableEnemyLunges) {
			return;
		}

		if (!npc.HasValidTarget) {
			return;
		}

		var target = npc.GetTarget();

		if (target == null) {
			// They played us like a damn fiddle!
			return;
		}

		var npcCenter = npc.Center;
		var targetCenter = target.Center;

		// Forbid leaping backwards.
		if (npc.direction != ((targetCenter.X - npcCenter.X) >= 0f ? 1 : -1)) {
			return;
		}

		float distance = Vector2.Distance(targetCenter, npcCenter);

		if (npc.velocity.Y == 0f && distance <= 80f && prevDistance > 80f) {
			npc.velocity.X = 3f * npc.direction;
			npc.velocity.Y = -4f;

			if (!Main.dedServ) {
				npc.IdleSounds();
			}
		}

		prevDistance = distance;
	}
}

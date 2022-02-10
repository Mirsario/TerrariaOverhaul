using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Utilities.Extensions;

namespace TerrariaOverhaul.Common.ModEntities.NPCs.AI
{
	// This makes fighter AIs jump at their targets when they get close enough.
	public class NPCFighterJumpAttacks : GlobalNPC
	{
		private float prevDistance;

		public override bool InstancePerEntity => true;

		public override bool AppliesToEntity(NPC npc, bool lateInstantiation)
			=> lateInstantiation && npc.aiStyle == NPCAIStyleID.Fighter;

		public override void AI(NPC npc)
		{
			if (!npc.HasValidTarget) {
				return;
			}

			var target = npc.GetTarget();
			float distance = Vector2.Distance(target.Center, npc.Center);

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
}

using System;
using Terraria;
using Terraria.ModLoader;

namespace TerrariaOverhaul.Common.BloodAndGore;

[Autoload(Side = ModSide.Client)]
public class NPCBleeding : GlobalNPC
{
	public override bool PreAI(NPC npc)
	{
		// Bleed on low health.
		if (!Main.dedServ && npc.life < npc.lifeMax / 2 && (Main.GameUpdateCount + npc.whoAmI * 15) % 5 == 0) {
			NPCBloodAndGore.Bleed(npc, 1);
		}

		return true;
	}

	public override void OnKill(NPC npc)
	{
		// Add extra blood on death.
		if (!Main.dedServ) {
			int count = (int)Math.Sqrt(npc.width * npc.height) / 12;

			NPCBloodAndGore.Bleed(npc, count);
		}
	}
}

//Commented out, as I'm not sure about this feature --Mirsario.
/*using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace TerrariaOverhaul.Common.ModEntities.NPCs
{
	public class NPCFreezeFrames : GlobalNPC
	{
		public int FreezeFrames { get; private set; }

		public override bool InstancePerEntity => true;

		public override void Load()
		{
			On.Terraria.NPC.UpdateNPC += (orig, npc, i) => {
				if(npc.TryGetGlobalNPC<NPCFreezeFrames>(out var globalNPC) && globalNPC.FreezeFrames > 0) {
					globalNPC.FreezeFrames--;
					return;
				}

				orig(npc, i);
			};
		}

		public void SetFreezeFrames(NPC npc, int ticks)
		{
			FreezeFrames = Math.Max(ticks, FreezeFrames);
		}
	}
}*/

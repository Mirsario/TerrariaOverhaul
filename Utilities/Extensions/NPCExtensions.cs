using Terraria;

namespace TerrariaOverhaul.Utilities.Extensions
{
	public static class NpcExtensions
	{
		public static Entity GetTarget(this NPC npc)
		{
			if(!npc.HasValidTarget) {
				return null;
			}

			return npc.HasPlayerTarget ? Main.player[npc.target] : (Entity)Main.npc[npc.target - 300];
		}
	}
}

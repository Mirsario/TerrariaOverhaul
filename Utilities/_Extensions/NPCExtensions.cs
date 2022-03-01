using Terraria;

namespace TerrariaOverhaul.Utilities
{
	public static class NpcExtensions
	{
		public static Entity GetTarget(this NPC npc)
		{
			if (!npc.HasValidTarget) {
				return null;
			}

			return npc.HasPlayerTarget ? Main.player[npc.target] : Main.npc[npc.target - 300];
		}
	}
}

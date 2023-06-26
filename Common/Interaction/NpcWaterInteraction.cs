using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.Camera;
using TerrariaOverhaul.Utilities;
using static TerrariaOverhaul.Common.Interaction.WaterAndRainInteractions;

namespace TerrariaOverhaul.Common.Interaction;

public sealed class NpcWaterInteraction : GlobalNPC
{
	public override void Load()
	{
		On_NPC.UpdateNPC_BuffApplyVFX += NpcVfxDetour;
	}

	public override void PostAI(NPC npc)
	{
		bool ThresholdCheck()
		{
			int debuffIndex = npc.FindBuffIndex(DebuffId);

			return debuffIndex < 0 || npc.buffTime[debuffIndex] <= DebuffTimeThreshold;
		}

		if (Main.netMode == NetmodeID.MultiplayerClient || !LiquidCheck(npc)) {
			// Rain collision checks only run on clients, for on-screen NPCs.
			if (Main.netMode == NetmodeID.Server || !CameraSystem.ScreenRect.Intersects(npc.GetRectangle()) || !ThresholdCheck() || !RainCheck(npc)) {
				return;
			}
		} else if (!ThresholdCheck()) {
			return;
		}

		npc.AddBuff(DebuffId, DebuffTime, quiet: false);
	}

	// Fix for dripping particles being spawned while underwater
	private static void NpcVfxDetour(On_NPC.orig_UpdateNPC_BuffApplyVFX original, NPC npc)
	{
		npc.dripping &= !npc.wet;

		original(npc);
	}
}

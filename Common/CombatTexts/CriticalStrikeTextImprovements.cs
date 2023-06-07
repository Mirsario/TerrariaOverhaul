using Terraria;
using Terraria.ModLoader;

namespace TerrariaOverhaul.Common.CombatTexts;

public sealed class CriticalStrikeTextImprovements : ModSystem
{
	public override void Load()
	{
		On_NPC.StrikeNPC_HitInfo_bool_bool += NPC_StrikeNPC;
	}

	private static int NPC_StrikeNPC(On_NPC.orig_StrikeNPC_HitInfo_bool_bool orig, NPC self, NPC.HitInfo hitInfo, bool fromNet, bool noPlayerInteraction)
	{
		if (hitInfo.Crit) {
			CombatTextSystem.AddFilter(1, text => {
				if (uint.TryParse(text.text, out _) && !text.text.Contains('!')) {
					text.text += "!";
				}
			});
		}

		return orig(self, hitInfo, fromNet, noPlayerInteraction);
	}
}

using Terraria;
using Terraria.ModLoader;

namespace TerrariaOverhaul.Common.CombatTexts;

public sealed class CriticalStrikeTextImprovements : ModSystem
{
	public override void Load()
	{
		On_NPC.StrikeNPC += NPC_StrikeNPC;
	}

	private static double NPC_StrikeNPC(On_NPC.orig_StrikeNPC orig, NPC self, int damage, float knockback, int hitDirection, bool crit, bool noEffect, bool fromNet)
	{
		if (crit) {
			CombatTextSystem.AddFilter(1, text => {
				if (uint.TryParse(text.text, out _) && !text.text.Contains('!')) {
					text.text += "!";
				}
			});
		}

		return orig(self, damage, knockback, hitDirection, crit, noEffect, fromNet);
	}
}

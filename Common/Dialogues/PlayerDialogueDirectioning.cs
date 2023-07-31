using System;
using Terraria;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.Movement;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.Dialogues;

// Forces players to face their conversation partners.
// This also fixes inability to pet pets.
public sealed class PlayerDialogueDirectioning : ModPlayer
{
	public override void PreUpdate()
	{
		if (Player.TalkNPC is NPC { active: true } npc && Player.TryGetModPlayer(out PlayerDirectioning directions)) {
			var playerCenter = Player.Center;
			var npcCenter = npc.Center;

			directions.SetDirectionOverride(npcCenter.X > playerCenter.X ? Direction1D.Right : Direction1D.Left, 3);
			directions.SetLookPositionOverride(npcCenter, 3);
		}
	}
}

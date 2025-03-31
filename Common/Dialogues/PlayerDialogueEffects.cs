// Copyright (c) 2020-2025 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using Terraria;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.Camera;
using TerrariaOverhaul.Common.Movement;
using TerrariaOverhaul.Core.Configuration;
using TerrariaOverhaul.Utilities.Terraria;

namespace TerrariaOverhaul.Common.Dialogues;

// Forces players to face their conversation partners.
// This also fixes inability to pet pets.
public sealed class PlayerDialogueEffects : ModPlayer
{
	private static readonly ConfigEntry<bool> FocusCameraOnDialogues = new(ConfigSide.ClientOnly, true, "Camera");

	public override void PreUpdate()
	{
		if (Player.TalkNPC is NPC { active: true } npc) {
			if (Player.TryGetModPlayer(out PlayerDirection directions)) {
				var playerCenter = Player.Center;
				var npcCenter = npc.Center;

				directions.SetDirectionOverride(npcCenter.X > playerCenter.X ? Direction1D.Right : Direction1D.Left, 3);
				directions.SetLookPositionOverride(npcCenter, 3);
			}

			if (!Main.dedServ && FocusCameraOnDialogues) {
				CameraCurios.Create(npc.Center, new() {
					Zoom = +1f,
					Weight = 1.5f,
					LengthInSeconds = 0.1f,
					FadeInLength = 0.40f,
					FadeOutLength = 0.40f,
					UniqueId = "TalkNPC",
				});
			}
		}
	}
}

// Copyright (c) 2020-2025 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using Terraria;
using Terraria.ModLoader;
using TerrariaOverhaul.Core.Configuration;
using TerrariaOverhaul.Core.Time;
using TerrariaOverhaul.Utilities.Xna;

namespace TerrariaOverhaul.Common.Movement;

internal sealed class PlayerJumpBuffering : ModPlayer
{
	public static readonly ConfigEntry<bool> EnableJumpInputBuffering = new(ConfigSide.ClientOnly, true, "Accessibility", "Movement");

	public float JumpKeyBuffer { get; private set; }

	public override void Load()
	{
		On_Player.JumpMovement += JumpMovement;
	}

	public override void PostUpdate()
	{
		if (!EnableJumpInputBuffering) {
			return;
		}

		JumpKeyBuffer = MathUtils.StepTowards(JumpKeyBuffer, 0f, TimeSystem.LogicDeltaTime);
	}

	public override void SetControls()
	{
		if (!EnableJumpInputBuffering) {
			return;
		}

		if (Player.controlJump && Player.releaseJump && Player.velocity.Y != 0f) {
			JumpKeyBuffer = 0.25f;
		}
	}

	private static void JumpMovement(On_Player.orig_JumpMovement orig, Player player)
	{
		if (!EnableJumpInputBuffering) {
			orig(player);
			return;
		}

		var modPlayer = player.GetModPlayer<PlayerJumpBuffering>();
		bool forceJump = !player.controlJump && modPlayer.JumpKeyBuffer > 0f && player.velocity.Y == 0f;
		bool originalControlJump = player.controlJump;
		bool originalAutoJump = player.autoJump;

		if (forceJump) {
			modPlayer.JumpKeyBuffer = 0f;

			player.controlJump = player.autoJump = true;
		}

		orig(player);

		if (forceJump) {
			player.controlJump = originalControlJump;
			player.autoJump = originalAutoJump;
		}
	}
}

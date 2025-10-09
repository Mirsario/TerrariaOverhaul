// Copyright (c) 2020-2025 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using System;
using Terraria;
using Terraria.ModLoader;
using TerrariaOverhaul.Core.Configuration;
using TerrariaOverhaul.Utilities.Terraria;

namespace TerrariaOverhaul.Common.Movement;

internal sealed class PlayerCoyoteTime : ModPlayer
{
	public static readonly ConfigEntry<bool> EnableCoyoteTime = new(ConfigSide.Both, true, "Movement", "Accessibility");

	public GameTimer Timer;
	public uint DurationInTicks;

	public override void ResetEffects()
	{
		DurationInTicks = 13;
	}

	public override void Load()
	{
		On_Player.JumpMovement += JumpMovementHook;
	}

	public override void PostUpdate()
	{
		if (!EnableCoyoteTime) {
			return;
		}

		// Start the coyote timer when the player starts falling off a ledge.
		if (Player.oldVelocity.Y == 0f && Player.velocity.Y > 0f) {
			Timer.Set(DurationInTicks);
		}

		// Animation cue.
		if (!Main.dedServ && Timer.Value > 3 && Player.velocity.Y > 0f && Player.bodyFrame.Y == ((int)PlayerFrames.Jump * Player.bodyFrame.Height)) {
			Player.bodyFrame.Y = (int)PlayerFrames.Walk5 * Player.bodyFrame.Height;
		}
	}

	private static void JumpMovementHook(On_Player.orig_JumpMovement orig, Player player)
	{
		if (!EnableCoyoteTime) {
			orig(player);
			return;
		}

		var modPlayer = player.GetModPlayer<PlayerCoyoteTime>();

		// Only allow coyote jump if the player is airborne, the coyote window is active,
		// and the player is actually pressing the jump key.
		bool allowCoyote = player.velocity.Y != 0f && player.controlJump && modPlayer.Timer.Active;

		if (allowCoyote) {
			// Perform the coyote jump just like a normal jump.
			player.velocity.Y = Math.Min(player.velocity.Y, -Player.jumpSpeed * player.gravDir);
			player.jump = Math.Max(player.jump, Player.jumpHeight);
			player.releaseJump = false;
			modPlayer.Timer = default;
		} else {
			orig(player);
		}
	}
}

// Copyright (c) 2020-2025 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.Dodgerolls;
using TerrariaOverhaul.Common.Footsteps;
using TerrariaOverhaul.Common.Tags;
using TerrariaOverhaul.Core.Configuration;
using TerrariaOverhaul.Core.Time;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.Movement;

// This class implements both wall jumps and wall rolls.
public class PlayerWallJumps : ModPlayer
{
	public static readonly ConfigEntry<bool> EnableWallJumping = new(ConfigSide.Both, true, "Movement");
	public static readonly ConfigEntry<bool> EnableWallFlips = new(ConfigSide.Both, true, "Movement");

	public bool CanWallRoll = true;
	public bool CanWallJump = false;
	public Stats WallRoll = new() {
		DirectionMultiplier = -1,
		InputDirection = new(-1, 0),
		MinTriggerVelocity = new(4.00f, 0.00f),
		BaseVelocity = new(2.00f, -8.00f),
		TranslatedVelocity = new(0.75f, 0.00f),
		TriggerDodgeroll = true,
	};
	public Stats WallJump = new() {
		DirectionMultiplier = 1,
		InputDirection = new(0, -1),
		MinTriggerVelocity = new(1.00f, 0.00f),
		BaseVelocity = new(5.00f, -7.45f),
		TranslatedVelocity = new(0.00f, 0.00f),
	};

	public struct Stats
	{
		public required int DirectionMultiplier;
		public required (sbyte? X, sbyte? Y) InputDirection;
		public required Vector2 MinTriggerVelocity;
		public required Vector2 BaseVelocity;
		public required Vector2 TranslatedVelocity;
		public bool SpawnEffects = true;
		public bool TriggerDodgeroll = false;

		public Stats() { }
	}

	public override void ResetEffects()
	{
		CanWallRoll = true;
		CanWallJump = Player.EnumerateAccessories().Any(tuple => OverhaulItemTags.NinjaGear.Has(tuple.item.type));
	}

	public override bool PreItemCheck()
	{
		// Wall Jumps are executed by looking away from a wall while touching it and pressing movement towards it.
		if (EnableWallJumping && CanWallJump) {
			if (TryDoingWallJump(in WallJump)) {
				return true;
			}
		}
		
		if (EnableWallFlips && CanWallRoll) {
			if (TryDoingWallJump(in WallRoll)) {
				return true;
			}
		}

		return true;
	}

	public bool TryDoingWallJump(in Stats stats)
	{
		if (!Player.IsLocal() || Player.mount.Active || Player.pulley || Player.EnumerateGrapplingHooks().Any()) {
			return false;
		}

		// Short-circuit if dodging or if no velocity data is available.
		var playerDodgerolls = Player.GetModPlayer<PlayerDodgerolls>();
		var playerMovement = Player.GetModPlayer<PlayerMovement>();
		if (playerDodgerolls.IsDodging || playerMovement.VelocityRecord == null) {
			return false;
		}

		// Check inputs
		var keyDirection = Player.KeyDirection();
		var keySigns = new Vector2Int(Math.Sign(keyDirection.X), Math.Sign(keyDirection.Y));
		if ((stats.InputDirection.X != null && stats.InputDirection.X.Value != keySigns.X * Player.direction)
		|| (stats.InputDirection.Y != null && stats.InputDirection.Y.Value != keySigns.Y)) {
			return false;
		}

		// Return if the player didn't JUST hit a wall, or if they're standing on the ground.
		if (Player.velocity.X != 0f || Player.oldVelocity.X == 0f || Player.OnGround()) {
			return false;
		}

		// Check that the player's *velocity* direction wasn't changed recently, to prevent wall jumps from getting spammed every frame.
		int jumpDirection = Player.direction * stats.DirectionMultiplier;
		sbyte prevDirX = (sbyte)(Player.oldVelocity.X > 0f ? 1 : -1);
		if (Player.direction != prevDirX * -stats.DirectionMultiplier) {
			return false;
		}

		// Summ up recent speed values and check if they're enough to trigger the wall move.
		var maxRecentSpeeds = default(Vector2);
		foreach (var velocity in playerMovement.VelocityRecord) {
			maxRecentSpeeds.X = Math.Max(maxRecentSpeeds.X, Math.Abs(velocity.X));
			maxRecentSpeeds.Y = Math.Max(maxRecentSpeeds.Y, Math.Abs(velocity.Y));
		}
		if (maxRecentSpeeds.X < stats.MinTriggerVelocity.X || maxRecentSpeeds.Y < stats.MinTriggerVelocity.Y) {
			return false;
		}

		// Collision checks.
		var tilePos = Player.position.ToTileCoordinates();
		if (!TileCheckUtils.CheckIfAllBlocksAreSolid(tilePos.X + (jumpDirection == 1 ? -1 : 2), tilePos.Y + 1, 1, 2)) {
			return false;
		}

		// Apply velocity.
		Player.velocity.X = jumpDirection * Math.Max(stats.BaseVelocity.X, maxRecentSpeeds.X * stats.TranslatedVelocity.X);
		Player.velocity.Y = Math.Min(Player.velocity.Y, Math.Min(stats.BaseVelocity.Y, -1f * maxRecentSpeeds.Y * stats.TranslatedVelocity.Y));

		// Effects.
		if (!Main.dedServ && stats.SpawnEffects) {
			// Spawn dusts.
			for (int i = 0; i < 12; i++) {
				Dust.NewDust(prevDirX > 0 ? Player.Right : Player.Left, 4, 12, DustID.Smoke, -prevDirX);
			}

			// Do a footstep sound.
			var footPoint = Player.TopLeft.ToTileCoordinates16() + new Point16(prevDirX < 0 ? -1 : 2, 1);
			FootstepSystem.Footstep(Player, FootstepType.Jump, footPoint);
		}

		if (stats.TriggerDodgeroll) {
			playerDodgerolls.QueueDodgeroll((uint)(TimeSystem.LogicFramerate * 0.1f), (Direction1D)(-prevDirX), force: true);
		}

		Player.StopGrappling();

		return true;
	}
}

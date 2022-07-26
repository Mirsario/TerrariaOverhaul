using System;
using System.Linq;
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
	public static readonly ConfigEntry<bool> EnableWallJumping = new(ConfigSide.Both, "PlayerMovement", nameof(EnableWallJumping), () => true);
	public static readonly ConfigEntry<bool> EnableWallFlips = new(ConfigSide.Both, "PlayerMovement", nameof(EnableWallFlips), () => true);

	public const float MinSpeedForWallRoll = 3f;

	public override bool PreItemCheck()
	{
		TryDoingWallJumps();

		return true;
	}

	private void TryDoingWallJumps()
	{
		if (!Player.IsLocal() || Player.mount.Active || Player.pulley || Player.EnumerateGrapplingHooks().Any()) {
			return;
		}

		var playerDodgerolls = Player.GetModPlayer<PlayerDodgerolls>();
		var playerMovement = Player.GetModPlayer<PlayerMovement>();

		if (playerDodgerolls.IsDodging || playerMovement.VelocityRecord == null) {
			return;
		}

		// Ninja jumps are executed by looking away from a wall while touching it and pressing movement towards it.
		// They do not trigger dodgerolls.
		bool ninjaJump = Player.controlUp && Player.EnumerateAccessories().Any(tuple => OverhaulItemTags.NinjaGear.Has(tuple.item.type));

		if (!(ninjaJump ? EnableWallJumping : EnableWallFlips)) {
			return;
		}

		var ordereredVelocityRecord = playerMovement.VelocityRecord.OrderBy(q => q.X);

		sbyte prevDirX = (sbyte)(Player.oldVelocity.X > 0f ? 1 : -1);
		float fastestSpeed = prevDirX < 0 ? ordereredVelocityRecord.First().X : ordereredVelocityRecord.Last().X;

		// Return if the player didn't JUST hit a wall, or if they're standing on the ground.
		if (Player.velocity.X != 0f || Player.oldVelocity.X == 0f || Player.OnGround()) {
			return;
		}

		if (Math.Abs(fastestSpeed) < MinSpeedForWallRoll && !ninjaJump || Player.direction != (ninjaJump ? -prevDirX : prevDirX) || Player.KeyDirection() != (ninjaJump ? 0 : -Player.direction)) {
			return;
		}

		var tilePos = Player.position.ToTileCoordinates();
		bool frontWallSolid = TileCheckUtils.CheckIfAllBlocksAreSolid(tilePos.X + (Player.direction == 1 ? 2 : -1), tilePos.Y + 1, 1, 2);
		bool backWallSolid = TileCheckUtils.CheckIfAllBlocksAreSolid(tilePos.X + (Player.direction == 1 ? -1 : 2), tilePos.Y + 1, 1, 2);

		if (ninjaJump) {
			if (!TileCheckUtils.CheckIfAllBlocksAreSolid(tilePos.X + (Player.direction == 1 ? -1 : 2), tilePos.Y + 1, 1, 2)) {
				return;
			}
		} else {
			if (!TileCheckUtils.CheckIfAllBlocksAreSolid(tilePos.X + (Player.direction == 1 ? 2 : -1), tilePos.Y + 1, 1, 2)) {
				return;
			}
		}

		Player.velocity.X = ninjaJump ? 5f * -prevDirX : -(fastestSpeed * 0.75f);
		Player.velocity.Y = Math.Min(Player.velocity.Y, ninjaJump ? -7.45f : -8f);

		if (!Main.dedServ) {
			// Play voicelines.

			//TODO:
			/*var slot = audioSource["voice"];

			if(slot.instance != null) {
				slot.SetSound(entity => SoundInstance.Create<OggSoundInstance, OggSoundInfo>("Voice/ArgLong", entity, 1f, (player.Male ? Main.rand.Range(1f, 1.25f) : Main.rand.Range(1.55f, 1.75f)) * 1.2f));
			}*/

			// Spawn dusts.

			for (int i = 0; i < 12; i++) {
				Dust.NewDust(prevDirX > 0 ? Player.Right : Player.Left, 4, 12, DustID.Smoke, -prevDirX);
			}

			// Do a footstep sound.

			var footPoint = Player.TopLeft.ToTileCoordinates16() + new Point16(prevDirX < 0 ? -1 : 2, 1);

			//DebugSystem.DrawTileRect(footPoint, Color.ForestGreen);

			FootstepSystem.Footstep(Player, FootstepType.Jump, footPoint);
		}

		if (!ninjaJump) {
			playerDodgerolls.QueueDodgeroll((uint)(TimeSystem.LogicFramerate * 0.1f), (sbyte)-prevDirX, force: true);
		}

		Player.StopGrappling();

		/*if(Main.netMode != NetmodeID.SinglePlayer) {
			MultiplayerSystem.SendPacket(new PlayerSetVelocityMessage(player, player.velocity));
		}*/
	}
}

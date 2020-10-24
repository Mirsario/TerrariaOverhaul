using System;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.Systems.Footsteps;
using TerrariaOverhaul.Common.Tags;
using TerrariaOverhaul.Utilities;
using TerrariaOverhaul.Utilities.Extensions;

namespace TerrariaOverhaul.Common.ModEntities.Players
{
	//This class implements both wall jumps and wall rolls.
	public class PlayerWallJumps : ModPlayer
	{
		public const float MinSpeedForWallRoll = 3f;

		public override bool PreItemCheck()
		{
			if(!player.IsLocal() || player.mount.Active || player.pulley || player.EnumerateGrapplingHooks().Any()) {
				return true;
			}

			var playerMovement = player.GetModPlayer<PlayerMovement>();

			if(playerMovement.velocityRecord == null) {
				return true;
			}

			var ordereredVelocityRecord = playerMovement.velocityRecord.OrderBy(q => q.X);
			sbyte prevDirX = player.oldVelocity.X > 0f ? (sbyte)1 : (sbyte)-1;
			float fastestSpeed = prevDirX < 0 ? ordereredVelocityRecord.First().X : ordereredVelocityRecord.Last().X;

			//Ninja jumps are executed by looking away from a wall while touching it and pressing movement towards it.
			//They do not trigger dodgerolls.
			bool ninjaJump = player.controlUp && player.EnumerateAccessories().Any(tuple => OverhaulItemTags.NinjaGear.Has(tuple.item.type));

			//Return if the player didn't JUST hit a wall, or if they're standing on the ground.
			if(player.velocity.X != 0f || player.oldVelocity.X == 0f || player.OnGround()) {
				return true;
			}

			var playerDodgerolls = player.GetModPlayer<PlayerDodgerolls>();

			if(playerDodgerolls.isDodging) {
				return true;
			}

			if((Math.Abs(fastestSpeed) < MinSpeedForWallRoll && !ninjaJump) || player.direction != (ninjaJump ? -prevDirX : prevDirX) || player.KeyDirection() != (ninjaJump ? 0 : -player.direction)) {
				return true;
			}

			var tilePos = player.position.ToTileCoordinates();
			bool frontWallSolid = TileCheckUtils.CheckIfAllBlocksAreSolid(tilePos.X + (player.direction == 1 ? 2 : -1), tilePos.Y + 1, 1, 2);
			bool backWallSolid = TileCheckUtils.CheckIfAllBlocksAreSolid(tilePos.X + (player.direction == 1 ? -1 : 2), tilePos.Y + 1, 1, 2);

			if(ninjaJump) {
				if(!TileCheckUtils.CheckIfAllBlocksAreSolid(tilePos.X + (player.direction == 1 ? -1 : 2), tilePos.Y + 1, 1, 2)) {
					return true;
				}
			} else {
				if(!TileCheckUtils.CheckIfAllBlocksAreSolid(tilePos.X + (player.direction == 1 ? 2 : -1), tilePos.Y + 1, 1, 2)) {
					return true;
				}
			}

			player.velocity.X = ninjaJump ? 5f * -prevDirX : -(fastestSpeed * 0.75f);
			player.velocity.Y = Math.Min(player.velocity.Y, ninjaJump ? -7.45f : -8f);
			//lastRealJumpPress = Main.GlobalTime;

			if(!Main.dedServ) {
				//Play voicelines.

				//TODO:
				/*var slot = audioSource["voice"];

				if(slot.instance != null) {
					slot.SetSound(entity => SoundInstance.Create<OggSoundInstance, OggSoundInfo>("Voice/ArgLong", entity, 1f, (player.Male ? Main.rand.Range(1f, 1.25f) : Main.rand.Range(1.55f, 1.75f)) * 1.2f));
				}*/

				//Spawn dusts.

				for(int i = 0; i < 12; i++) {
					Dust.NewDust(prevDirX > 0 ? player.Right : player.Left, 4, 12, DustID.Smoke, -prevDirX);
				}

				//Do a footstep sound.

				var footPoint = player.TopLeft.ToTileCoordinates16() + new Point16(prevDirX < 0 ? -1 : 2, 1);

				//DebugSystem.DrawTileRect(footPoint, Color.ForestGreen);

				FootstepSystem.Foostep(player, footPoint);
			}

			if(!ninjaJump) {
				playerDodgerolls.QueueDodgeroll(0.1f, (sbyte)-prevDirX, true);
			}

			player.StopGrappling();

			/*if(Main.netMode != NetmodeID.SinglePlayer) {
				MultiplayerSystem.SendPacket(new PlayerSetVelocityMessage(player, player.velocity));
			}*/

			return true;
		}
	}
}

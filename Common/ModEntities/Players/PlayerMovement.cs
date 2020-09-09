using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using TerrariaOverhaul.Utilities.Extensions;

namespace TerrariaOverhaul.Common.ModEntities.Players
{
	public class PlayerMovement : OverhaulPlayer
	{
		public static readonly int VelocityRecordSize = 5;
		public static readonly float DefaultJumpSpeedScale = 1.52375f;
		public static readonly float UnderwaterJumpSpeedScale = 0.775f;

		public bool noMovement;
		public int vanillaAccelerationTime;
		public int forcedDirection;
		public Vector2? forcedPosition;
		public Vector2 prevVelocity;
		public Vector2[] velocityRecord = new Vector2[VelocityRecordSize];

		public override void PreUpdate()
		{
			bool onGround = player.OnGround();
			bool wasOnGround = player.WasOnGround();

			player.fullRotationOrigin = new Vector2(11,22);

			if(onGround || player.wet) {
				Player.jumpHeight = 0;

				if(!player.chilled && !player.slowFall) {
					Player.jumpSpeed *= DefaultJumpSpeedScale;
				}

				if(player.wet) {
					Player.jumpSpeed *= UnderwaterJumpSpeedScale;
				}
			}

			if(!player.wet) {
				bool wings = player.wingsLogic>0 && player.controlJump && !onGround && !wasOnGround;
				bool wingFall = wings && player.wingTime==0;

				if(vanillaAccelerationTime>0) {
					vanillaAccelerationTime--;
				} else if(!player.slippy && !player.slippy2) {
					//Run acceleration
					if(onGround) {
						player.runAcceleration *= 2f;
					}

					//Wind acceleration
					if(player.FindBuffIndex(BuffID.WindPushed)>=0) {
						if(Main.windSpeedCurrent>=0f ? player.velocity.X<Main.windSpeedCurrent : player.velocity.X>Main.windSpeedCurrent) {
							player.velocity.X += Main.windSpeedCurrent/(player.KeyDirection()==-Math.Sign(Main.windSpeedCurrent) ? 180f : 70f);
						}
					}

					player.runSlowdown = onGround ? 0.3f : /* TODO: isDodging true ? 0.125f : */ 0.02f;
				}

				//Stops vanilla running sounds from playing. //TODO: Move to PlayerFootsteps.
				player.runSoundDelay = 5;

				if(noMovement) {
					noMovement = false;
					player.maxRunSpeed = 0f;
					player.runAcceleration = 0f;
				} else if(player.chilled) {
					player.maxRunSpeed *= 0.6f;
				}

				player.maxFallSpeed = wingFall ? 10f : 1000f;

				if(player.velocity.Y>player.maxFallSpeed) {
					player.velocity.Y = player.maxFallSpeed;
				} else if(player.velocity.Y>0f) {
					player.velocity.Y *= 0.995f;
				}
			}
		}
		public override void PostUpdate()
		{
			Array.Copy(velocityRecord,0,velocityRecord,1,velocityRecord.Length-1); //Shift

			velocityRecord[0] = player.velocity;

			if(forcedPosition!=null) {
				player.position = forcedPosition.Value;
				forcedPosition = null;
			}

			player.oldVelocity = player.velocity;
		}

		/*public override bool PreItemCheck()
		{
			SetDirection();

			return true;
		}*/
	}
}

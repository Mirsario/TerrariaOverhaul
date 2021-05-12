using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using TerrariaOverhaul.Common.Systems.Time;
using TerrariaOverhaul.Utilities.DataStructures;
using TerrariaOverhaul.Utilities.Extensions;

namespace TerrariaOverhaul.Common.ModEntities.Players
{
	public sealed class PlayerMovement : PlayerBase
	{
		public struct MovementModifier
		{
			public static MovementModifier Default = new() {
				gravityScale = 1f,
				runAccelerationScale = 1f
			};

			public float gravityScale;
			public float runAccelerationScale;

			public void Apply(Player player)
			{
				player.gravity *= gravityScale;
				player.runAcceleration *= runAccelerationScale;
			}
		}

		public static readonly int VelocityRecordSize = 5;

		//public Timer noJumpTime;
		public Timer noMovementTime;
		public int vanillaAccelerationTime;
		public Vector2? forcedPosition;
		//public Vector2 prevVelocity;
		public Vector2[] velocityRecord = new Vector2[VelocityRecordSize];

		private Dictionary<string, (ulong endTime, MovementModifier modifier)> movementModifiers = new();

		public override void PreUpdate()
		{
			bool onGround = Player.OnGround();
			bool wasOnGround = Player.WasOnGround();

			Player.fullRotationOrigin = new Vector2(11, 22);

			//Scale jump stats
			Player.jumpHeight = 0;
			Player.jumpSpeed *= 1.23f;

			//Jump hold logic is replaced by gravity scaling
			if(Player.controlJump && Player.velocity.Y < 0f) {
				Player.gravity *= MathHelper.Lerp(1f, 0.19f, -Player.velocity.Y / Player.jumpSpeed);
			}

			if(!Player.wet) {
				bool wings = Player.wingsLogic > 0 && Player.controlJump && !onGround && !wasOnGround;
				bool wingFall = wings && Player.wingTime == 0;

				if(vanillaAccelerationTime > 0) {
					vanillaAccelerationTime--;
				} else if(!Player.slippy && !Player.slippy2) {
					//Horizontal acceleration
					if(onGround) {
						Player.runAcceleration *= 2f;
					} else {
						Player.runAcceleration *= 1.65f;
					}

					//Wind acceleration
					if(Player.FindBuffIndex(BuffID.WindPushed) >= 0) {
						if(Main.windSpeedCurrent >= 0f ? Player.velocity.X < Main.windSpeedCurrent : Player.velocity.X > Main.windSpeedCurrent) {
							Player.velocity.X += Main.windSpeedCurrent / (Player.KeyDirection() == -Math.Sign(Main.windSpeedCurrent) ? 180f : 70f);
						}
					}

					Player.runSlowdown = onGround ? 0.275f : 0.02f;
				}

				//Stops vanilla running sounds from playing. //TODO: Move to PlayerFootsteps.
				Player.runSoundDelay = 5;

				if(noMovementTime.Active) {
					Player.maxRunSpeed = 0f;
					Player.runAcceleration = 0f;
				} else if(Player.chilled) {
					Player.maxRunSpeed *= 0.6f;
				}

				Player.maxFallSpeed = wingFall ? 10f : 1000f;

				//Falling friction & speed limit
				if(Player.velocity.Y > Player.maxFallSpeed) {
					Player.velocity.Y = Player.maxFallSpeed;
				} else if(Player.velocity.Y > 0f) {
					Player.velocity.Y *= 0.995f;
				}
			}

			HandleMovementModifiers();
		}
		public override void PostUpdate()
		{
			Array.Copy(velocityRecord, 0, velocityRecord, 1, velocityRecord.Length - 1); //Shift

			velocityRecord[0] = Player.velocity;

			if(forcedPosition != null) {
				Player.position = forcedPosition.Value;
				forcedPosition = null;
			}

			Player.oldVelocity = Player.velocity;
		}

		public void SetMovementModifier(string id, int time, MovementModifier modifier)
		{
			movementModifiers.TryGetValue(id, out var tuple);

			tuple.endTime = Math.Max(tuple.endTime, TimeSystem.UpdateCount + (ulong)time);
			tuple.modifier = modifier;

			movementModifiers[id] = tuple;
		}

		private void HandleMovementModifiers()
		{
			List<string> keysToRemove = null;

			foreach(var pair in movementModifiers) {
				string id = pair.Key;
				var (endTime, modifier) = pair.Value;

				if(endTime <= TimeSystem.UpdateCount) {
					(keysToRemove ??= new List<string>()).Add(id);
					continue;
				}

				modifier.Apply(Player);
			}

			if(keysToRemove != null) {
				foreach(string key in keysToRemove) {
					movementModifiers.Remove(key);
				}
			}
		}
	}
}

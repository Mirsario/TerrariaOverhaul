using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.ModEntities.Players.Packets;
using TerrariaOverhaul.Common.Systems.Time;
using TerrariaOverhaul.Core.Systems.Input;
using TerrariaOverhaul.Core.Systems.Networking;
using TerrariaOverhaul.Utilities;
using TerrariaOverhaul.Utilities.DataStructures;
using TerrariaOverhaul.Utilities.Enums;
using TerrariaOverhaul.Utilities.Extensions;

namespace TerrariaOverhaul.Common.ModEntities.Players
{
	public sealed class PlayerDodgerolls : ModPlayer
	{
		public static readonly ISoundStyle DodgerollSound = new ModSoundStyle($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Player/Armor", 3, volume: 0.65f, pitchVariance: 0.2f);

		public static ModKeybind DodgerollKey { get; private set; }

		public static float DodgeTimeMax => 0.37f;
		public static uint DodgeDefaultCooldown => 90;

		public Timer dodgeCooldown;
		public sbyte dodgeDirection;
		public sbyte dodgeDirectionVisual;
		public sbyte wantedDodgerollDir;
		public float dodgeTime;
		public float dodgeStartRot;
		public float dodgeItemRotation;
		public bool isDodging;
		public float wantsDodgerollTimer;
		public bool forceDodgeroll;
		public bool noDodge;

		public override void Load()
		{
			DodgerollKey = KeybindLoader.RegisterKeybind(Mod, "Dodgeroll", Keys.LeftControl);
		}
		
		public override bool PreItemCheck()
		{
			UpdateDodging();

			//Stop umbrella and other things from working
			if(isDodging && Player.HeldItem.type == ItemID.Umbrella) {
				return false;
			}

			return true;
		}

		//CanX
		public override bool CanBeHitByNPC(NPC npc, ref int cooldownSlot) => !isDodging;
		public override bool CanBeHitByProjectile(Projectile proj) => !isDodging;
		public override bool CanUseItem(Item item) => !isDodging;

		public void QueueDodgeroll(float wantTime, sbyte direction, bool force = false)
		{
			wantsDodgerollTimer = wantTime;
			wantedDodgerollDir = direction;

			if(force) {
				dodgeCooldown = 0;
			}
		}

		private bool TryStartDodgeroll()
		{
			bool isLocal = Player.IsLocal();

			if(isLocal && wantsDodgerollTimer <= 0f && DodgerollKey.JustPressed && !Player.mouseInterface) {
				QueueDodgeroll(0.25f, (sbyte)Player.KeyDirection());
			}

			if(!forceDodgeroll) {
				//Only initiate dodgerolls locally.
				if(!isLocal) {
					return false;
				}

				//Input & cooldown check. The cooldown can be enforced by other actions.
				if(wantsDodgerollTimer <= 0f || dodgeCooldown.Active) {
					return false;
				}

				//Don't allow dodging on mounts and during item use.
				if((Player.mount != null && Player.mount.Active) || Player.itemAnimation > 0) {
					return false;
				}
			}

			wantsDodgerollTimer = 0f;

			/*if(onFire) {
				//Don't stop but roll
				int fireBuff = player.FindBuffIndex(24);
				int fireBuff2 = player.FindBuffIndex(39);
			
				if(fireBuff!=-1) {
					player.buffTime[fireBuff] -= 90;
				}
			
				if(fireBuff2!=-1) {
					player.buffTime[fireBuff2] -= 90;
				}
			}*/

			if(!Main.dedServ) {
				SoundEngine.PlaySound(DodgerollSound, Player.Center);
			}

			Player.StopGrappling();

			Player.eocHit = 1;

			isDodging = true;
			dodgeStartRot = Player.GetModPlayer<PlayerRotation>().rotation;
			dodgeItemRotation = Player.itemRotation;
			dodgeTime = 0f;
			dodgeDirectionVisual = (sbyte)Player.direction;
			dodgeDirection = wantedDodgerollDir != 0 ? wantedDodgerollDir : (sbyte)Player.direction;
			dodgeCooldown = DodgeDefaultCooldown;

			if(!isLocal) {
				forceDodgeroll = false;
			} else if(Main.netMode != NetmodeID.SinglePlayer) {
				MultiplayerSystem.SendPacket(new PlayerDodgerollPacket(Player));
			}

			return true;
		}
		private void UpdateDodging()
		{
			wantsDodgerollTimer = MathUtils.StepTowards(wantsDodgerollTimer, 0f, TimeSystem.LogicDeltaTime);

			noDodge |= Player.mount.Active;

			if(noDodge) {
				isDodging = false;
				noDodge = false;

				return;
			}

			bool onGround = Player.OnGround();
			bool wasOnGround = Player.WasOnGround();

			ref float rotation = ref Player.GetModPlayer<PlayerRotation>().rotation;

			//Attempt to initiate a dodgeroll if the player isn't doing one already.
			if(!isDodging && !TryStartDodgeroll()) {
				return;
			}

			//Lower fall damage
			if(dodgeTime < DodgeTimeMax / 1.5f && onGround && !wasOnGround) {
				Player.fallStart = (int)MathHelper.Lerp(Player.fallStart, (int)(Player.position.Y / 16f), 0.35f);
			}

			//Open doors
			var tilePos = Player.position.ToTileCoordinates16();
			int x = dodgeDirection > 0 ? tilePos.X + 2 : tilePos.X - 1;

			for(int y = tilePos.Y; y < tilePos.Y + 3; y++) {
				if(!Main.tile.TryGet(x, y, out var tile)) {
					continue;
				}

				if(tile.type == TileID.ClosedDoor) {
					WorldGen.OpenDoor(x, y, dodgeDirection);
				}
			}

			//Apply velocity
			if(dodgeTime < DodgeTimeMax * 0.5f) {
				float newVelX = (onGround ? 6f : 4f) * dodgeDirection;

				if(Math.Abs(Player.velocity.X) < Math.Abs(newVelX) || Math.Sign(newVelX) != Math.Sign(Player.velocity.X)) {
					Player.velocity.X = newVelX;
				}
			}

			if(!Main.dedServ) {
				//Trail
				Player.GetModPlayer<PlayerEffects>().ForceTrailEffect(2);
			}

			Player.pulley = false;

			//Apply rotations & direction
			Player.GetModPlayer<PlayerItemRotation>().forcedItemRotation = dodgeItemRotation;
			Player.GetModPlayer<PlayerAnimations>().forcedLegFrame = PlayerFrames.Jump;
			Player.GetModPlayer<PlayerDirectioning>().forcedDirection = dodgeDirectionVisual;

			rotation = dodgeDirection == 1
				? Math.Min(MathHelper.Pi * 2f, MathHelper.Lerp(dodgeStartRot, MathHelper.TwoPi, dodgeTime / (DodgeTimeMax * 1f)))
				: Math.Max(-MathHelper.Pi * 2f, MathHelper.Lerp(dodgeStartRot, -MathHelper.TwoPi, dodgeTime / (DodgeTimeMax * 1f)));

			//Progress the dodgeroll
			dodgeTime += 1f / 60f;

			//Prevent other actions
			Player.GetModPlayer<PlayerClimbing>().climbCooldown.Set(1);

			if(dodgeTime >= DodgeTimeMax) {
				isDodging = false;
				Player.eocDash = 0;

				//forceSyncControls = true;
			} else {
				Player.runAcceleration = 0f;
			}
		}
	}
}

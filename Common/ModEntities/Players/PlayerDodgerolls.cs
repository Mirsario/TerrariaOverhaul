using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.ModEntities.Players.Packets;
using TerrariaOverhaul.Common.Systems.Time;
using TerrariaOverhaul.Core.Systems.Input;
using TerrariaOverhaul.Core.Systems.Networking;
using TerrariaOverhaul.Utilities;
using TerrariaOverhaul.Utilities.Enums;
using TerrariaOverhaul.Utilities.Extensions;

namespace TerrariaOverhaul.Common.ModEntities.Players
{
	public sealed class PlayerDodgerolls : ModPlayer
	{
		public static readonly SoundStyle DodgerollSound = new ModSoundStyle(nameof(TerrariaOverhaul),"Assets/Sounds/Player/Armor",3,volume:0.65f,pitchVariance:0.2f);

		public static float DodgeTimeMax => 0.37f;
		public static float DodgeDefaultCooldown => 1.5f;

		public int dodgeStartBodyFrame;
		public sbyte dodgeDirection;
		public sbyte dodgeDirectionVisual;
		public sbyte wantedDodgerollDir;
		public float dodgeTime;
		public float dodgeStartRot;
		public float dodgeCooldown;
		public float dodgeItemRotation;
		public bool isDodging;
		public float wantsDodgerollTimer;
		public bool forceDodgeroll;
		public bool noDodge;

		public override bool PreItemCheck()
		{
			UpdateDodging();

			//Stop umbrella and other things from working
			if(isDodging && player.HeldItem.type==ItemID.Umbrella) {
				return false;
			}

			return true;
		}
		public override bool CanBeHitByNPC(NPC npc,ref int cooldownSlot) => !isDodging;
		public override bool CanBeHitByProjectile(Projectile proj) => !isDodging;

		public void QueueDodgeroll(float wantTime,sbyte direction,bool force = false)
		{
			wantsDodgerollTimer = wantTime;
			wantedDodgerollDir = direction;

			if(force) {
				dodgeCooldown = 0f;
			}
		}

		private void UpdateDodging()
		{

			dodgeCooldown = MathUtils.StepTowards(dodgeCooldown,0f,TimeSystem.LogicDeltaTime);
			wantsDodgerollTimer = MathUtils.StepTowards(wantsDodgerollTimer,0f,TimeSystem.LogicDeltaTime);

			noDodge |= player.mount.Active;

			if(noDodge) {
				isDodging = false;
				noDodge = false;

				return;
			}

			bool isLocal = player.IsLocal();
			bool onGround = player.OnGround();
			bool wasOnGround = player.WasOnGround();

			ref float rotation = ref player.GetModPlayer<PlayerRotation>().rotation;

			if(!isDodging) {
				if(isLocal && wantsDodgerollTimer<=0f && InputSystem.KeyDodgeroll.JustPressed) {
					QueueDodgeroll(0.25f,(sbyte)player.KeyDirection());
				}

				if(forceDodgeroll || isLocal && wantsDodgerollTimer>0f && dodgeCooldown<=0f && !noDodge && (player.mount==null || !player.mount.Active) && (player.itemAnimation<=0)) { // || ignoreItemAnim
					wantsDodgerollTimer = 0f;

					//if(onFire) {
					//	//Don't stop but roll
					//	int fireBuff = player.FindBuffIndex(24);
					//	int fireBuff2 = player.FindBuffIndex(39);
					//
					//	if(fireBuff!=-1) {
					//		player.buffTime[fireBuff] -= 90;
					//	}
					//
					//	if(fireBuff2!=-1) {
					//		player.buffTime[fireBuff2] -= 90;
					//	}
					//}

					if(!Main.dedServ) {
						SoundEngine.PlaySound(DodgerollSound,player.Center);
					}

					//player.StopGrappling(); //TODO:
					player.pulley = false;

					isDodging = true;
					dodgeStartRot = rotation;
					dodgeItemRotation = player.itemRotation;
					dodgeTime = 0f;
					dodgeStartBodyFrame = player.bodyFrame.Y/56;
					dodgeDirectionVisual = (sbyte)player.direction;
					dodgeDirection = wantedDodgerollDir!=0 ? wantedDodgerollDir : (sbyte)player.direction;
					dodgeCooldown = DodgeDefaultCooldown;

					player.eocHit = 1;

					if(!isLocal) {
						forceDodgeroll = false;
					} else if(Main.netMode!=NetmodeID.SinglePlayer) {
						MultiplayerSystem.SendPacket(new PlayerDodgerollPacket(player));
					}
				}
			} else {
				player.GetModPlayer<PlayerItems>().canUseItems = true;

				//Lower fall damage
				if(dodgeTime<DodgeTimeMax/1.5f && onGround && !wasOnGround) {
					player.fallStart = (int)MathHelper.Lerp(player.fallStart,(int)(player.position.Y/16f),0.35f);
				}

				var tilePos = player.position.ToTileCoordinates16();

				//Open doors
				int x = dodgeDirection>0 ? tilePos.X+2 : tilePos.X-1;

				for(int y = tilePos.Y;y<tilePos.Y+3;y++) {
					if(!Main.tile.TryGet(x,y,out var tile)) {
						continue;
					}

					if(tile.type==TileID.ClosedDoor) {
						WorldGen.OpenDoor(x,y,dodgeDirection);
					}
				}

				if(dodgeTime<DodgeTimeMax*0.5f) {
					float newVelX = (onGround ? 6f : 4f)*dodgeDirection;
					if(Math.Abs(player.velocity.X)<Math.Abs(newVelX) || Math.Sign(newVelX)!=Math.Sign(player.velocity.X)) {
						player.velocity.X = newVelX;
					}
				}

				if(!Main.dedServ) {
					//Trail
					player.GetModPlayer<PlayerEffects>().ForceTrailEffect(2);
				}

				player.pulley = false;

				player.GetModPlayer<PlayerItemRotation>().forcedItemRotation = dodgeItemRotation;
				player.GetModPlayer<PlayerAnimations>().forcedLegFrame = PlayerFrames.Jump;
				player.GetModPlayer<PlayerDirectioning>().forcedDirection = dodgeDirectionVisual;

				rotation = dodgeDirection==1
					? Math.Min(MathHelper.Pi*2f,MathHelper.Lerp(dodgeStartRot,MathHelper.TwoPi,dodgeTime/(DodgeTimeMax*1f)))
					: Math.Max(-MathHelper.Pi*2f,MathHelper.Lerp(dodgeStartRot,-MathHelper.TwoPi,dodgeTime/(DodgeTimeMax*1f)));

				dodgeTime += 1f/60f;

				if(dodgeTime>=DodgeTimeMax) {
					isDodging = false;
					player.eocDash = 0;

					//forceSyncControls = true;
				} else {
					player.runAcceleration = 0f;
				}
			}
		}
	}
}

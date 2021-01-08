using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.Systems.Camera.ScreenShakes;
using TerrariaOverhaul.Common.Systems.Time;
using TerrariaOverhaul.Core.Systems.Configuration;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.Systems.Camera
{
	public sealed class CameraSystem : ModSystem
	{
		private struct FocusInfo
		{
			public Vector2 position;
			public Vector2 velocity;
		}

		private static FocusInfo? focus;
		private static uint cameraUpdatePrevUpdateCount;
		private static float originalZoom;
		private static float smoothZoomScale = 1f;
		private static Vector2 prevOffsetGoal;
		private static Stopwatch cameraUpdateSW;
		private static Vector2 cameraShakeOffset;
		private static Vector2 screenPos;
		private static Vector2 screenPosNoShakes;
		private static Vector2 cameraSubFrameOffset;
		private static Vector2 oldCameraPos;
		private static Vector2 oldCameraOffset;

		public static CameraConfig Config => ConfigSystem.GetConfig<CameraConfig>();
		public static Vector2 ScreenSize => new Vector2(Main.screenWidth, Main.screenHeight);
		public static Vector2 ScreenHalf => new Vector2(Main.screenWidth * 0.5f, Main.screenHeight * 0.5f);
		public static Rectangle ScreenRect => new Rectangle((int)Main.screenPosition.X, (int)Main.screenPosition.Y, Main.screenWidth, Main.screenHeight);
		public static Rectangle ScreenRectExtra => new Rectangle((int)Main.screenPosition.X - Main.offScreenRange, (int)Main.screenPosition.Y - Main.offScreenRange, Main.screenWidth + Main.offScreenRange * 2, Main.screenHeight + Main.offScreenRange * 2);
		public static Rectangle ScreenRectExtraHalf => new Rectangle((int)Main.screenPosition.X - Main.offScreenRange / 2, (int)Main.screenPosition.Y - Main.offScreenRange / 2, Main.screenWidth + Main.offScreenRange, Main.screenHeight + Main.offScreenRange);
		public static Vector2 MouseWorld => Main.MouseWorld - cameraShakeOffset;
		public static Vector2 ScreenCenter {
			get => new Vector2(Main.screenPosition.X + Main.screenWidth * 0.5f, Main.screenPosition.Y + Main.screenHeight * 0.5f);
			set => Main.screenPosition = new Vector2(value.X - Main.screenWidth * 0.5f, value.Y - Main.screenHeight * 0.5f);
		}

		private int followNPC = -1;
		private bool noOffsetUpdating;
		/*private int screenWidthTileSpace;
		private int screenHeightTileSpace;
		private Point screenPosTilePostDraw; //some cache
		private Vector2 screenPosPlusOffscreen;
		private Rectangle screenRect;
		private Rectangle screenRectExtra;

		public override void Unload()
		{
			screenPosPlusOffscreen = new Vector2(Main.screenPosition.X+Main.offScreenRange,Main.screenPosition.Y+Main.offScreenRange);
			screenPosTilePostDraw = new Point((int)Main.screenPosition.X-Main.offScreenRange,(int)(Main.screenPosition.Y-Main.offScreenRange+2f));
			screenRect = ScreenRect;
			screenRectExtra = ScreenRectExtra;
			screenWidthTileSpace = Main.screenWidth/16;
			screenHeightTileSpace = Main.screenHeight/16;
		}*/

		public void OnEnterWorld()
		{
			cameraUpdateSW = null;

			smoothZoomScale = 3f;
		}
		public void OnLeaveWorld()
		{
			cameraUpdateSW = null;
		}

		public override void ModifyScreenPosition()
		{
			if(Main.gameMenu) {
				return;
			}

			var player = Main.LocalPlayer;

			if(player?.active != true) {
				return;
			}

			var config = Config;
			var currentFocus = focus ?? GetFocusFor(player);

			//NaNCheck
			if(Main.screenPosition.HasNaNs()) {
				ResetCamera();
			}

			float frameDelta = TimeSystem.LogicDeltaTime;

			if(cameraUpdateSW == null) {
				cameraUpdateSW = new Stopwatch();

				cameraUpdateSW.Start();
			} else {
				cameraUpdateSW.Stop();

				frameDelta = (float)cameraUpdateSW.Elapsed.TotalSeconds;

				cameraUpdateSW.Reset();
				cameraUpdateSW.Start();
			}

			if(Main.GameZoomTarget < 1f) {
				Main.GameZoomTarget = originalZoom = 1f;
			} else if(originalZoom == 0f) {
				originalZoom = 1f;
			}

			uint newUpdateCount = Main.GameUpdateCount;
			bool skip = Main.FrameSkipMode == 0 && cameraUpdatePrevUpdateCount == newUpdateCount && !Main.gamePaused;

			cameraUpdatePrevUpdateCount = newUpdateCount;

			Main.SetCameraLerp(1f, 0);

			float zoomScaleGoal = 1f;
			float mouseMovementScale = config.fixedCamera ? 0f : 1f;

			const float ReducedOffsetTime = 1f;

			float worldTime = TimeSystem.UpdateCount * TimeSystem.LogicDeltaTime;

			if(worldTime < ReducedOffsetTime) {
				mouseMovementScale *= worldTime / ReducedOffsetTime;
			}

			/*if(player.dead && (followNPC>0 && (entity = Main.npc[followNPC])!=null && entity.active && ((NPC)entity).HasTag(NPCTags.Zombie))) {
				if(!skip) {
					cameraFocusPoint = entity.Center;
					cameraFocusVelocity = entity.velocity;
				}

				mouseMovementScale = 0f;
			} else if(oPlayer.isSleeping) {
				mouseMovementScale = 0f;
				zoomScaleGoal = 2f/originalZoom;
				cameraFocusPoint = player.Top;
				cameraFocusVelocity = default;
			} else {
				followNPC = -1;

				bool posAtPlayer = true;

				if(player.talkNPC>=0 && (npc = Main.npc[player.talkNPC])!=null && npc.active) {
					mouseMovementScale = 0f;

					if(ConfigSystem.local.Clientside.Camera.dialogueZoomIn) {
						zoomScaleGoal = 2f/originalZoom;
					}

					if(ConfigSystem.local.Clientside.Camera.dialogueFixCamera) {
						posAtPlayer = false;
						if(!skip) {
							cameraFocusPoint = Vector2.Lerp(npc.Center,player.Center,0.5f);
							cameraFocusVelocity = player.velocity;
						}
					}
				}

				if(!skip && posAtPlayer) {
					cameraFocusPoint = player.Center;
					cameraFocusVelocity = player.velocity;
				}
			}*/

			if(!skip) {
				cameraSubFrameOffset = default;
			} else if(currentFocus.velocity.X != 0f || currentFocus.velocity.Y != 0f) {
				cameraSubFrameOffset += currentFocus.velocity * frameDelta;
			}

			if(!skip) {
				if(zoomScaleGoal != smoothZoomScale) {
					smoothZoomScale = MathUtils.StepTowards(smoothZoomScale, zoomScaleGoal, 2f * TimeSystem.LogicDeltaTime);
					Main.GameZoomTarget = originalZoom * smoothZoomScale;
				} else if(zoomScaleGoal == 1f) {
					originalZoom = Main.GameZoomTarget;
				} else if(smoothZoomScale != 1f) {
					Main.GameZoomTarget = originalZoom * smoothZoomScale;
				}
			}

			float length = 0.15f;
			var item = player.HeldItem;

			if(item != null && item.active && item.stack > 0 && item.type == ItemID.SniperRifle && player.controlUseTile) {
				length = 0.5f;
			}

			screenPosNoShakes = currentFocus.position - ScreenHalf;
			screenPosNoShakes.X += Main.cameraX;
			screenPosNoShakes.Y += player.gfxOffY;

			Vector2 mousePos = new Vector2(
				Main.mouseX,
				player.gravDir > 0f ? Main.mouseY : Main.screenHeight - Main.mouseY
			);

			Vector2 offset;

			if(noOffsetUpdating || !Main.hasFocus || Main.mouseX < 0 || Main.mouseX > Main.screenWidth || Main.mouseY < 0 || Main.mouseY > Main.screenHeight) {
				offset = prevOffsetGoal;
			} else {
				if(player.dead || player.whoAmI == Main.myPlayer && (Main.playerInventory || Main.ingameOptionsWindow)) {
					offset = default;
				} else {
					offset = (mousePos - ScreenHalf) * new Vector2(length, Math.Sign(player.gravity) * length);
				}
			}

			noOffsetUpdating = false;
			prevOffsetGoal = offset;

			if(mouseMovementScale == 0f) {
				offset = default;
			}

			offset = Vector2.Lerp(oldCameraOffset, offset, MathHelper.Clamp(20f * frameDelta, 0f, 1f));

			screenPosNoShakes += offset * mouseMovementScale;

			oldCameraOffset = offset;

			float shakePower = ScreenShakeSystem.GetPowerAtPoint(ScreenCenter) * 0.5f;

			cameraShakeOffset = Main.rand.NextVector2Circular(shakePower, shakePower);
			screenPosNoShakes += cameraSubFrameOffset;

			if(!skip) {
				oldCameraPos = Vector2.Lerp(oldCameraPos, screenPosNoShakes, TimeSystem.LogicDeltaTime * 8f);
			}

			if(config.smoothCamera) {
				screenPosNoShakes += oldCameraPos - screenPosNoShakes;
			}

			if(mouseMovementScale > 0f) {
				screenPosNoShakes += offset * mouseMovementScale;
			}

			screenPos = screenPosNoShakes + cameraShakeOffset;

			if(screenPos.HasNaNs()) {
				ResetCamera();
			} else {
				Main.screenPosition = screenPos.Floor(); //new Vector2((float)Math.Round(screenPos.X),(float)Math.Round(screenPos.Y));
			}

			focus = null;
		}

		public static void ResetCamera()
		{
			var newFocus = new FocusInfo();

			Main.screenPosition = oldCameraPos = newFocus.position = screenPosNoShakes = screenPos = Main.LocalPlayer.Center - ScreenHalf;
			newFocus.velocity = oldCameraOffset = cameraShakeOffset = prevOffsetGoal = cameraSubFrameOffset = Vector2.Zero;

			focus = newFocus;
		}
		public static void SetFocus(Entity entity) => focus = GetFocusFor(entity);

		private static FocusInfo GetFocusFor(Entity entity) => new FocusInfo {
			position = entity.position,
			velocity = entity.velocity
		};
	}
}

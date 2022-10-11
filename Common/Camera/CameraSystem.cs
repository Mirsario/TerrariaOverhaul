using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Core.Configuration;
using TerrariaOverhaul.Core.Time;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.Camera;

//TODO: Split into multiple systems tbh.
public sealed class CameraSystem : ModSystem
{
	/*
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
	private static Stopwatch? cameraUpdateSW;
	private static Vector2 cameraShakeOffset;
	private static Vector2 screenPos;
	private static Vector2 screenPosNoShakes;
	private static Vector2 cameraSubFrameOffset;
	private static Vector2 oldCameraPos;
	private static Vector2 oldCameraOffset;
	*/

	public static Vector2 ScreenSize => new(Main.screenWidth, Main.screenHeight);
	public static Vector2 ScreenHalf => new(Main.screenWidth * 0.5f, Main.screenHeight * 0.5f);
	public static Rectangle ScreenRect => new((int)Main.screenPosition.X, (int)Main.screenPosition.Y, Main.screenWidth, Main.screenHeight);
	public static Vector2 MouseWorld => Main.MouseWorld;
	public static Vector2 ScreenCenter {
		get => new(Main.screenPosition.X + Main.screenWidth * 0.5f, Main.screenPosition.Y + Main.screenHeight * 0.5f);
		set => Main.screenPosition = new Vector2(value.X - Main.screenWidth * 0.5f, value.Y - Main.screenHeight * 0.5f);
	}

	/*
	private bool noOffsetUpdating;

	public override void ModifyScreenPosition()
	{
		if (Main.gameMenu) {
			return;
		}

		var player = Main.LocalPlayer;

		if (player?.active != true) {
			return;
		}

		var currentFocus = focus ?? GetFocusFor(player);

		// NaNCheck
		if (Main.screenPosition.HasNaNs()) {
			ResetCamera();
		}

		float frameDelta = TimeSystem.LogicDeltaTime;

		if (cameraUpdateSW == null) {
			cameraUpdateSW = new Stopwatch();

			cameraUpdateSW.Start();
		} else {
			cameraUpdateSW.Stop();

			frameDelta = (float)cameraUpdateSW.Elapsed.TotalSeconds;

			cameraUpdateSW.Reset();
			cameraUpdateSW.Start();
		}

		if (Main.GameZoomTarget < 1f) {
			Main.GameZoomTarget = originalZoom = 1f;
		} else if (originalZoom == 0f) {
			originalZoom = 1f;
		}

		uint newUpdateCount = Main.GameUpdateCount;
		bool skip = Main.FrameSkipMode == 0 && cameraUpdatePrevUpdateCount == newUpdateCount && !Main.gamePaused;

		cameraUpdatePrevUpdateCount = newUpdateCount;

		Main.SetCameraLerp(1f, 0);

		float zoomScaleGoal = 1f;
		float mouseMovementScale = 0f;

		const float ReducedOffsetTime = 1f;

		float worldTime = TimeSystem.UpdateCount * TimeSystem.LogicDeltaTime;

		if (worldTime < ReducedOffsetTime) {
			mouseMovementScale *= worldTime / ReducedOffsetTime;
		}

		if (!skip) {
			cameraSubFrameOffset = default;
		} else if (currentFocus.velocity.X != 0f || currentFocus.velocity.Y != 0f) {
			cameraSubFrameOffset += currentFocus.velocity * frameDelta;
		}

		if (!skip) {
			if (zoomScaleGoal != smoothZoomScale) {
				smoothZoomScale = MathUtils.StepTowards(smoothZoomScale, zoomScaleGoal, 2f * TimeSystem.LogicDeltaTime);
				Main.GameZoomTarget = originalZoom * smoothZoomScale;
			} else if (zoomScaleGoal == 1f) {
				originalZoom = Main.GameZoomTarget;
			} else if (smoothZoomScale != 1f) {
				Main.GameZoomTarget = originalZoom * smoothZoomScale;
			}
		}

		float length = 0.15f;
		var item = player.HeldItem;

		if (item != null && item.active && item.stack > 0 && item.type == ItemID.SniperRifle && player.controlUseTile) {
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

		if (noOffsetUpdating || !Main.hasFocus || Main.mouseX < 0 || Main.mouseX > Main.screenWidth || Main.mouseY < 0 || Main.mouseY > Main.screenHeight) {
			offset = prevOffsetGoal;
		} else {
			if (player.dead || player.whoAmI == Main.myPlayer && (Main.playerInventory || Main.ingameOptionsWindow)) {
				offset = default;
			} else {
				offset = (mousePos - ScreenHalf) * new Vector2(length, Math.Sign(player.gravity) * length);
			}
		}

		noOffsetUpdating = false;
		prevOffsetGoal = offset;

		if (mouseMovementScale == 0f) {
			offset = default;
		}

		offset = Vector2.Lerp(oldCameraOffset, offset, MathHelper.Clamp(20f * frameDelta, 0f, 1f));

		screenPosNoShakes += offset * mouseMovementScale;

		oldCameraOffset = offset;

		float shakePower = ScreenShakeSystem.GetPowerAtPoint(ScreenCenter) * 0.5f;

		cameraShakeOffset = Main.rand.NextVector2Circular(shakePower, shakePower);
		screenPosNoShakes += cameraSubFrameOffset;

		if (!skip) {
			oldCameraPos = Vector2.Lerp(oldCameraPos, screenPosNoShakes, TimeSystem.LogicDeltaTime * 8f);
		}

		if (SmoothCamera.Value) {
			screenPosNoShakes += oldCameraPos - screenPosNoShakes;
		}

		if (mouseMovementScale > 0f) {
			screenPosNoShakes += offset * mouseMovementScale;
		}

		screenPos = screenPosNoShakes + cameraShakeOffset;

		if (screenPos.HasNaNs()) {
			ResetCamera();
		} else {
			Main.screenPosition = screenPos.Floor(); //new Vector2((float)Math.Round(screenPos.X),(float)Math.Round(screenPos.Y));
		}

		focus = null;
	}

	public static void OnEnterWorld()
	{
		cameraUpdateSW = null;

		smoothZoomScale = 3f;
	}

	public static void OnLeaveWorld()
	{
		cameraUpdateSW = null;
	}

	public static void ResetCamera()
	{
		var newFocus = new FocusInfo();

		Main.screenPosition = oldCameraPos = newFocus.position = screenPosNoShakes = screenPos = Main.LocalPlayer.Center - ScreenHalf;
		newFocus.velocity = oldCameraOffset = cameraShakeOffset = prevOffsetGoal = cameraSubFrameOffset = Vector2.Zero;

		focus = newFocus;
	}

	public static void SetFocus(Entity entity)
	{
		focus = GetFocusFor(entity);
	}

	private static FocusInfo GetFocusFor(Entity entity) => new() {
		position = entity.position,
		velocity = entity.velocity
	};
	*/
}

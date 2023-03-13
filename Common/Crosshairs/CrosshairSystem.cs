using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.UI;
using TerrariaOverhaul.Core.Time;

namespace TerrariaOverhaul.Common.Crosshairs;

[Autoload(Side = ModSide.Client)]
public sealed class CrosshairSystem : ModSystem
{
	private struct CrosshairImpulse
	{
		public float LengthInSeconds;
		public TimeSpan StartTime;
		public CrosshairEffects Effects; 
	}

	private struct CrosshairStyle
	{
		public Asset<Texture2D> Texture;
		public SpriteFrame SpriteFrame;
		public float Offset;
		public float Rotation;
		public Color InnerColor;
		public Color OuterColor;
	}

	private const int MaxImpulses = 32;

	// Base
	private static SpriteFrame crosshairBaseFrame = new(4, 2);
	private static Asset<Texture2D>? crosshairTexture;
	private static GameInterfaceLayer? crosshairInterfaceLayer;
	// Impulses
	private static int impulseCount;
	private static CrosshairImpulse[]? impulses;

	public override void Load()
	{
		impulses = new CrosshairImpulse[MaxImpulses];
		crosshairTexture = Mod.Assets.Request<Texture2D>("Assets/Textures/UI/Crosshair");
	}

	public override void Unload()
	{
		impulses = null;
		crosshairTexture = null;
	}

	public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
	{
		if (Main.LocalPlayer.mouseInterface || layers.Count == 0) {
			return;
		}

		if (!ShouldShowCursor()) {
			return;
		}

		int cursorOrEndIndex = layers.FindIndex(layer => layer.Name == "Vanilla: Cursor");

		if (cursorOrEndIndex < 0) {
			cursorOrEndIndex = layers.Count - 1;
		}

		var vanillaLayer = layers[cursorOrEndIndex];

		crosshairInterfaceLayer ??= new LegacyGameInterfaceLayer(vanillaLayer.Name, CrosshairInterfaceLayer, InterfaceScaleType.UI);
		layers[cursorOrEndIndex] = crosshairInterfaceLayer;
	}

	public static void AddImpulse(CrosshairEffects effects, float lengthInSeconds)
	{
		if (impulses == null || impulseCount >= MaxImpulses) {
			return;
		}

		if (!ShouldShowCursor()) {
			return;
		}

		CrosshairImpulse impulse;

		impulse.Effects = effects;
		impulse.StartTime = TimeSpan.Zero; // To be set later.
		impulse.LengthInSeconds = lengthInSeconds;

		impulses[impulseCount++] = impulse;
	}

	public static void ClearImpulses()
	{
		impulseCount = 0;
	}

	private static bool ShouldShowCursor()
	{
		if (Main.dedServ || Main.gameMenu) {
			return false;
		}

		if (Main.LocalPlayer?.HeldItem is not Item { IsAir: false } item) {
			return false;
		}

		if (!item.TryGetGlobalItem(out ItemCrosshairController itemCrosshair)) {
			return false;
		}

		return itemCrosshair.Enabled;
	}

	private static bool TryGetRenderData(out CrosshairStyle result)
	{
		if (impulses == null) {
			result = default;
			return false;
		}

		result = new CrosshairStyle {
			Texture = crosshairTexture!,
			SpriteFrame = crosshairBaseFrame,
			InnerColor = Main.cursorColor,
			OuterColor = Main.MouseBorderColor,
		};

		var currentTime = TimeSystem.CurrentTimeSpan;
		float currentTimeInSeconds = (float)TimeSystem.CurrentTimeSpan.TotalSeconds;

		for (int i = 0, j = 0, loopEnd = impulseCount; i < loopEnd; i++) {
			ref var impulse = ref impulses[i];
			ref readonly var effects = ref impulse.Effects;

			if (impulse.StartTime == TimeSpan.Zero) {
				impulse.StartTime = currentTime;
			}

			float startTimeInSeconds = (float)impulse.StartTime.TotalSeconds;
			float lengthInSeconds = impulse.LengthInSeconds;
			float timeSinceStartInSeconds = MathF.Max(0f, currentTimeInSeconds - startTimeInSeconds);

			bool GetFactor(float lengthMultiplier, out float result)
			{
				result = 1f - MathF.Min(1f, timeSinceStartInSeconds / (lengthInSeconds * lengthMultiplier));

				return float.IsNormal(result);
			}

			bool remove = true;

			void ApplyFloatEffect(ref float result, in (float Value, float LengthFactor) effect)
			{
				if (effect.Value != default && GetFactor(effect.LengthFactor, out float factor)) {
					result += effect.Value * factor;
					remove = false;
				}
			}

			void ApplyColorEffect(ref Color result, in (Color Value, float LengthFactor) effect)
			{
				if (effect.Value != default && GetFactor(effect.LengthFactor, out float factor)) {
					result = Color.Lerp(result, effect.Value, effect.Value.A * factor / 255f);
					remove = false;
				}
			}

			ApplyFloatEffect(ref result.Offset, in effects.Offset);
			ApplyFloatEffect(ref result.Rotation, in effects.Rotation);
			ApplyColorEffect(ref result.InnerColor, in effects.InnerColor);
			ApplyColorEffect(ref result.OuterColor, in effects.OuterColor);

			if (remove) {
				impulseCount--;
			} else {
				impulses[j++] = impulses[i];
			}
		}

		return true;
	}

	private static bool CrosshairInterfaceLayer()
	{
		if (!TryGetRenderData(out var renderData)) {
			return true;
		}

		if (crosshairTexture is not { IsLoaded: true, Value: Texture2D texture }) {
			return true;
		}

		int offset = (int)Math.Ceiling(renderData.Offset);

		for (int i = 0; i < 2; i++) {
			bool inner = i == 0;

			for (byte y = 0; y < 2; y++) {
				for (byte x = 0; x < 2; x++) {
					int xDir = x == 0 ? -1 : 1;
					int yDir = y == 0 ? -1 : 1;

					Vector2 vecOffset = new Vector2(offset * xDir, offset * yDir).RotatedBy(renderData.Rotation);
					var frame = crosshairBaseFrame.With((byte)(x + (inner ? 0 : 2)), y);
					var srcRect = frame.GetSourceRectangle(texture);
					var dstRect = new Rectangle(Main.mouseX + (int)vecOffset.X, Main.mouseY + (int)vecOffset.Y, srcRect.Width, srcRect.Height);
					var origin = new Vector2((1 - x) * srcRect.Width, (1 - y) * srcRect.Height);
					var color = inner ? renderData.InnerColor : renderData.OuterColor;

					Main.spriteBatch.Draw(texture, dstRect, srcRect, color, renderData.Rotation, origin, SpriteEffects.None, 0f);
				}
			}
		}

		return true;
	}
}

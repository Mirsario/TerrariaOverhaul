using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.UI;
using TerrariaOverhaul.Common.Hooks.Items;
using TerrariaOverhaul.Common.Systems.Time;

namespace TerrariaOverhaul.Common.Systems.Crosshairs
{
	[Autoload(Side = ModSide.Client)]
	public sealed class CrosshairSystem : ModSystem
	{
		private static Asset<Texture2D> crosshairTexture;
		private static SpriteFrame crosshairBaseFrame = new SpriteFrame(4, 2);
		private static float crosshairOffset;
		private static float crosshairRotation;
		private static Color crosshairColor;
		private static List<CrosshairImpulse> impulses;

		public static bool ShowCrosshair {
			get {
				if(Main.dedServ || Main.gameMenu) {
					return false;
				}

				var player = Main.LocalPlayer;
				var item = player?.HeldItem;

				if(item?.IsAir != false) {
					return false;
				}

				return HookShowItemCrosshair.Hook.Invoke(item, player);
			}
		}

		public override void Load()
		{
			impulses = new List<CrosshairImpulse>();
			crosshairTexture = Mod.GetTexture("Assets/Sprites/UI/Crosshair");
		}
		public override void PostUpdateEverything()
		{
			float totalOffset = 0f;
			float totalRot = 0f;

			Color color = Main.cursorColor;

			for(int i = 0; i < impulses.Count; i++) {
				var impulse = impulses[i];
				float progress = impulse.time / impulse.timeMax;

				if(impulse.reversed) {
					progress = 1f - progress;
				}

				totalOffset += impulse.strength * progress;
				totalRot += impulse.rotation * progress;

				if(impulse.color.HasValue) {
					var impulseColor = impulse.color.Value;

					color = Color.Lerp(color, impulseColor, impulseColor.A * progress / 255f);
				}

				impulse.time -= TimeSystem.LogicDeltaTime;

				if(impulse.time <= 0f) {
					impulses.RemoveAt(i--);
				} else {
					impulses[i] = impulse;
				}
			}

			crosshairOffset = totalOffset;
			crosshairRotation = totalRot;
			crosshairColor = color;
		}

		public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
		{
			if(!ShowCrosshair || Main.LocalPlayer.mouseInterface || layers.Count == 0) {
				return;
			}

			int cursorOrEndIndex = layers.FindIndex(layer => layer.Name == "Vanilla: Cursor");

			if(cursorOrEndIndex < 0) {
				cursorOrEndIndex = layers.Count - 1;
			}

			layers[cursorOrEndIndex] = new LegacyGameInterfaceLayer(
				layers[cursorOrEndIndex].Name,
				() => {
					int offset = (int)Math.Ceiling(crosshairOffset);

					for(int i = 0; i < 2; i++) {
						bool outline = i == 1;

						for(byte y = 0; y < 2; y++) {
							for(byte x = 0; x < 2; x++) {
								int xDir = x == 0 ? -1 : 1;
								int yDir = y == 0 ? -1 : 1;

								var texture = crosshairTexture.Value;
								Vector2 vecOffset = new Vector2(offset * xDir, offset * yDir).RotatedBy(crosshairRotation);
								var frame = crosshairBaseFrame.With((byte)(x + (outline ? 2 : 0)), y);
								var srcRect = frame.GetSourceRectangle(texture);
								var dstRect = new Rectangle(Main.mouseX + (int)vecOffset.X, Main.mouseY + (int)vecOffset.Y, srcRect.Width, srcRect.Height);
								var origin = new Vector2((1 - x) * srcRect.Width, (1 - y) * srcRect.Height);
								var color = outline ? Main.MouseBorderColor : crosshairColor;

								Main.spriteBatch.Draw(texture, dstRect, srcRect, color, crosshairRotation, origin, SpriteEffects.None, 0f);
							}
						}
					}

					return true;
				},
				InterfaceScaleType.UI
			);
		}

		public static void AddImpulse(float strength, float timeInSeconds, float rotation = 0f, Color? color = null, bool reversed = false, bool autoRotation = false)
			=> AddImpulse(new CrosshairImpulse(strength, timeInSeconds, rotation, color, reversed, autoRotation));

		public static void AddImpulse(CrosshairImpulse impulse) => impulses.Add(impulse);

		public static void ClearImpulses() => impulses.Clear();
	}
}

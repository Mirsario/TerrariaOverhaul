using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;
using TerrariaOverhaul.Core.Time;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.Dodgerolls;

[Autoload(Side = ModSide.Client)]
public sealed class DodgerollMeterUISystem : ModSystem
{
	private static readonly int TicksBeforeFading = (int)(TimeSystem.LogicFramerate * 1.5f);
	private static readonly int FadingLength = (int)(TimeSystem.LogicFramerate * 0.3f);
	private static readonly float Opacity = 0.675f;

	private static Asset<Texture2D>? meterTexture;
	private static LegacyGameInterfaceLayer? layer;
	private static Timer lastNotChargedTime;

	public override void Load()
	{
		meterTexture = Mod.Assets.Request<Texture2D>("Common/Dodgerolls/DodgerollMeter");

		layer = new LegacyGameInterfaceLayer($"{nameof(TerrariaOverhaul)}: Dodgeroll Meter", () => {
			if (!meterTexture.IsLoaded) {
				return true;
			}

			var player = Main.LocalPlayer;

			if (player?.active != true || !player.TryGetModPlayer(out PlayerDodgerolls dodgerolls)) {
				return true;
			}

			var texture = meterTexture.Value;
			var basePosition = player.Bottom + new Vector2(0f, 10f) - Main.screenPosition;
			int? forcedFrame = null;

			if (dodgerolls.CurrentCharges >= dodgerolls.MaxCharges) {
				// If fully charged
				int numTicksFullyChargedFor = -lastNotChargedTime.UnclampedValue;

				if (numTicksFullyChargedFor > TicksBeforeFading + FadingLength) {
					return true;
				}

				forcedFrame = 3 + (int)Math.Ceiling(Math.Max(numTicksFullyChargedFor - TicksBeforeFading, 0) / (float)FadingLength * 2);
			} else {
				// If not charged
				lastNotChargedTime.Value = 0;
			}

			var drawColor = new Color(Opacity, Opacity, Opacity, Opacity);

			for (int i = 0; i < dodgerolls.MaxCharges; i++) {
				int frame;

				if (forcedFrame.HasValue) {
					frame = forcedFrame.Value;
				} else if (i < dodgerolls.CurrentCharges) {
					// Charged
					frame = 3;
				} else {
					// Missing
					frame = 1;

					if (dodgerolls.DodgerollTirednessTimer.Value < 5) {
						// All are about to be recharged
						frame = 2;
					} else if (dodgerolls.CurrentCharges == 0) {
						// All are missing
						frame = 0;
					}
				}

				var srcRect = new Rectangle(frame * 12, 0, 12, 12);

				float x = ((dodgerolls.MaxCharges - 1) * -0.5f) + i;
				var drawPosition = basePosition + new Vector2(x * 10f, 0f);

				Main.spriteBatch.Draw(texture, drawPosition, srcRect, drawColor, 0f, new Vector2(6f, 6f), 1f, SpriteEffects.None, 0f);
			}

			return true;
		});
	}

	public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
	{
		if (layer == null) {
			return;
		}
		
		int preferredIndex = layers.FindIndex(l => l.Name == "Vanilla: Cursor");

		layers.Insert(preferredIndex < 0 ? 0 : preferredIndex, layer);
	}
}

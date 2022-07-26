using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.Magic;
using TerrariaOverhaul.Core.Time;

namespace TerrariaOverhaul.Content.Buffs;

public class ManaAbsorption : ModBuff
{
	private static float pulse;
	private static Asset<Texture2D>? meterTexture;
	private static Asset<Texture2D>? alternateTexture;

	public override void Load()
	{
		if (!Main.dedServ) {
			meterTexture = Mod.Assets.Request<Texture2D>("Content/Buffs/ManaAbsorption_Meter");
			alternateTexture = Mod.Assets.Request<Texture2D>("Content/Buffs/ManaAbsorption_Alternate");
		}
	}

	public override void SetStaticDefaults()
	{
		Main.buffNoTimeDisplay[Type] = true;
		BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
	}

	public override void ModifyBuffTip(ref string tip, ref int rare)
	{
		if (!Main.LocalPlayer.TryGetModPlayer(out PlayerManaRebalance manaRebalance)) {
			return;
		}
		
		tip = tip.Replace("{Multiplier}", manaRebalance.VelocityManaRegenMultiplier.ToString("0.00"));
	}

	public override void Update(Player player, ref int buffIndex)
	{
		if (player != Main.LocalPlayer || !Main.LocalPlayer.TryGetModPlayer(out PlayerManaRebalance manaRebalance)) {
			return;
		}

		pulse += (manaRebalance.VelocityManaRegenMultiplier - 1f) * TimeSystem.LogicDeltaTime;
	}

	public override bool PreDraw(SpriteBatch spriteBatch, int buffIndex, ref BuffDrawParams drawParams)
	{
		drawParams.DrawColor = Color.White; // Don't require hovering to draw at full opacity.

		return true;
	}

	public override void PostDraw(SpriteBatch spriteBatch, int buffIndex, BuffDrawParams drawParams)
	{
		if (!Main.LocalPlayer.TryGetModPlayer(out PlayerManaRebalance manaRebalance)) {
			return;
		}

		// Draw an alternate color texture with pulsing opacity
		if (alternateTexture?.IsLoaded == true) {
			float pulse01 = MathF.Sin(pulse * 10f) * 0.5f + 0.5f;
			var alternateFrameColor = new Color(pulse01, pulse01, pulse01, pulse01);

			Main.spriteBatch.Draw(alternateTexture.Value, drawParams.Position, alternateFrameColor);
		}

		// Draw the bonus intensity meter
		if (meterTexture?.IsLoaded == true) {
			const int FrameWidth = 32;
			const int FrameHeight = 8;
			const int NumTotalFrames = 16;
			const int NumForegroundFrames = NumTotalFrames - 1;

			var destRect = new Rectangle(
				(int)drawParams.Position.X,
				(int)drawParams.Position.Y + 34,
				FrameWidth,
				FrameHeight
			);
			
			// Background
			var backgroundSrcRect = new Rectangle(0, 0, FrameWidth, FrameHeight);

			Main.spriteBatch.Draw(meterTexture.Value, destRect, backgroundSrcRect, Color.White);

			// Foreground
			const float StartHue = 1.4f;
			const float EndHue = 0.8f;

			float intensity = manaRebalance.VelocityManaRegenIntensity;
			int foregroundFrame = 1 + (int)MathF.Round(intensity * (NumForegroundFrames - 1));

			// Foreground - Line
			float lineHue = MathHelper.Lerp(StartHue, EndHue, intensity);
			var lineColor = GetRainbowColor(lineHue);
			var lineSrcRect = new Rectangle(0, foregroundFrame * FrameHeight, FrameWidth, FrameHeight);

			Main.spriteBatch.Draw(meterTexture.Value, destRect, lineSrcRect, lineColor);
			
			// Foreground - Edge
			//float edgeHue = MathHelper.Clamp(lineHue + 0.1f, EndHue, StartHue);
			var edgeColor = Color.Lerp(lineColor, Color.White, 2f / 3f); //GetRainbowColor(edgeHue);
			var edgeSrcRect = new Rectangle(FrameWidth, foregroundFrame * FrameHeight, FrameWidth, FrameHeight);

			Main.spriteBatch.Draw(meterTexture.Value, destRect, edgeSrcRect, edgeColor);
		}
	}

	private static Color GetRainbowColor(float x)
	{
		float div = Math.Abs(x % 1) * 6f;
		float x1 = div % 1; // Ascending
		float x2 = 1f - x1; // Descending

		return (int)div switch {
			0 => new Color(1f, x1, 0f),
			1 => new Color(x2, 1f, 0f),
			2 => new Color(0f, 1f, x1),
			3 => new Color(0f, x2, 1f),
			4 => new Color(x1, 0f, 1f),
			_ => new Color(1f, 0f, x2),
		};
	}
}

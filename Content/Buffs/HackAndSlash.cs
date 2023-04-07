using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.Melee;

namespace TerrariaOverhaul.Content.Buffs;

public sealed class HackAndSlash : ModBuff
{
	private static Asset<Texture2D>? meterTexture;

	public override void Load()
	{
		if (!Main.dedServ) {
			meterTexture = Mod.Assets.Request<Texture2D>($"Content/Buffs/{nameof(HackAndSlash)}_Meter");
		}
	}

	public override void SetStaticDefaults()
	{
		Main.buffNoTimeDisplay[Type] = true;
		BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
	}

	public override void ModifyBuffText(ref string buffName, ref string tip, ref int rare)
	{
		if (Main.LocalPlayer is not Player { HeldItem: Item heldItem } player) {
			return;
		}

		if (!heldItem.TryGetGlobalItem(out ItemVelocityBasedDamage velocityDamage)) {
			return;
		}

		float damageMultiplier = velocityDamage.CalculateDamageMultiplier(player.velocity);

		tip = tip.Replace("{Multiplier}", damageMultiplier.ToString("0.00"));
	}

	public override bool PreDraw(SpriteBatch spriteBatch, int buffIndex, ref BuffDrawParams drawParams)
	{
		drawParams.DrawColor = Color.White; // Don't require hovering to draw at full opacity.

		return true;
	}

	public override void PostDraw(SpriteBatch spriteBatch, int buffIndex, BuffDrawParams drawParams)
	{
		if (Main.LocalPlayer is not Player { HeldItem: Item heldItem } player) {
			return;
		}

		if (!heldItem.TryGetGlobalItem(out ItemVelocityBasedDamage velocityDamage)) {
			return;
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
			float velocityFactor = velocityDamage.CalculateVelocityFactor(player.velocity);
			int foregroundFrame = 1 + (int)MathF.Round(velocityFactor * (NumForegroundFrames - 1));

			// Foreground - Line
			var lineColor = ItemVelocityBasedDamage.GetColorForVelocityFactor(velocityFactor);
			var lineSrcRect = new Rectangle(0, foregroundFrame * FrameHeight, FrameWidth, FrameHeight);

			Main.spriteBatch.Draw(meterTexture.Value, destRect, lineSrcRect, lineColor);
			
			// Foreground - Edge
			var edgeColor = Color.Lerp(lineColor, Color.White, 2f / 3f);
			var edgeSrcRect = new Rectangle(FrameWidth, foregroundFrame * FrameHeight, FrameWidth, FrameHeight);

			Main.spriteBatch.Draw(meterTexture.Value, destRect, edgeSrcRect, edgeColor);
		}
	}
}

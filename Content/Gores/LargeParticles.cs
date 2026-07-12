using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace TerrariaOverhaul.Content.Gores;

internal abstract class AnimatedGore : ModGore
{
	protected int maxTime = -1;

	public override bool Update(Gore gore)
	{
		if (maxTime < 0) maxTime = Math.Max(1, gore.timeLeft);
		
		gore.position += gore.velocity;

		int lastFrame = gore.Frame.RowCount - 1;
		var progress = 1f - (gore.timeLeft / (float)maxTime);
		int frameIndex = Math.Min((int)(progress * gore.Frame.RowCount), lastFrame);
		gore.Frame = gore.Frame.With(0, (byte)frameIndex);

		if (--gore.timeLeft <= 0)
		{
			gore.active = false;
		}

		return false;
	}
}

internal sealed class DustCloudSmall : AnimatedGore
{
	public override void OnSpawn(Gore gore, IEntitySource source)
	{
		gore.sticky = false;
		gore.behindTiles = true;
		gore.timeLeft = maxTime = Main.rand.Next(50, 60);
		gore.Frame = new SpriteFrame(1, 9) with { PaddingX = 0, PaddingY = 0 };
	}
}

internal sealed class DustCloudMedium : AnimatedGore
{
	public override void OnSpawn(Gore gore, IEntitySource source)
	{
		gore.sticky = false;
		gore.behindTiles = true;
		gore.timeLeft = maxTime = Main.rand.Next(100, 120);
		gore.Frame = new SpriteFrame(1, 13) with { PaddingX = 0, PaddingY = 0 };
	}
}

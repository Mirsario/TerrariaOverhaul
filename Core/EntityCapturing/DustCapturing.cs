// Copyright (c) 2020-2026 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using TerrariaOverhaul.Utilities;
using TerrariaOverhaul.Utilities.Xna;

namespace TerrariaOverhaul.Core.EntityCapturing;

public readonly record struct DustCapture(int Type, Vector2 Position, Vector2 Velocity, int Alpha, Color NewColor, float Scale);

internal sealed class DustCapturing : ModSystem
{
	private static readonly Stack<List<DustCapture>> listStack = new();
	private static Counter skipCounter;

	public override void Load()
	{
		On_Dust.NewDust += NewDustDetour;
	}

	public static CaptureHandle<DustCapture> Capture(List<DustCapture> captures)
		=> new(listStack, captures);

	public static CaptureHandle<DustCapture> Capture(out List<DustCapture> captures)
		=> Capture(captures = new());

	public static Counter.Handle Suspend()
		=> skipCounter.Increase();

	private static int NewDustDetour(On_Dust.orig_NewDust orig, Vector2 position, int width, int height, int type, float speedX, float speedY, int alpha, Color newColor, float scale)
	{
		if (listStack.TryPeek(out var list) && skipCounter.Active) {
			var randomPosition = Main.rand.NextVector2(position.X, position.Y, position.X + width, position.Y + height);
			var velocity = new Vector2(speedX, speedY);

			list.Add(new DustCapture(type, randomPosition, velocity, alpha, newColor, scale));

			return Main.maxItems;
		}

		return orig(position, width, height, type, speedX, speedY, alpha, newColor, scale);
	}
}

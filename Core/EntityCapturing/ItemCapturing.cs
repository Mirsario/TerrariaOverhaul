// Copyright (c) 2020-2026 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using TerrariaOverhaul.Utilities;
using TerrariaOverhaul.Utilities.Xna;

namespace TerrariaOverhaul.Core.EntityCapturing;

internal readonly record struct ItemCapture(IEntitySource Source, Vector2 Position, int Type, int Stack, int Prefix);

internal sealed class ItemCapturing : ModSystem
{
	private static readonly Stack<List<ItemCapture>> listStack = new();
	private static Counter skipCounter;

	public override void Load()
	{
		On_Item.NewItem_IEntitySource_int_int_int_int_int_int_bool_int_bool_bool += NewItemDetour;
	}

	public static CaptureHandle<ItemCapture> Capture(List<ItemCapture> captures)
		=> new(listStack, captures);

	public static CaptureHandle<ItemCapture> Capture(out List<ItemCapture> captures)
		=> Capture(captures = new());

	public static Counter.Handle Suspend()
		=> skipCounter.Increase();

	private static int NewItemDetour(On_Item.orig_NewItem_IEntitySource_int_int_int_int_int_int_bool_int_bool_bool orig, IEntitySource source, int x, int y, int width, int height, int type, int stack, bool noBroadcast, int prefix, bool noGrabDelay, bool reverseLookup)
	{
		if (skipCounter.Active) return Main.maxItems;

		if (listStack.TryPeek(out var list)) {
			list.Add(new ItemCapture(source, Main.rand.NextVector2(x, y, x + width, y + height), type, stack, prefix));

			return Main.maxItems;
		}

		return orig(source, x, y, width, height, type, stack, noBroadcast, prefix, noGrabDelay, reverseLookup);
	}
}

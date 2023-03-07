using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using TerrariaOverhaul.Utilities;
using On_Item = On.Terraria.Item;

namespace TerrariaOverhaul.Core.EntityCapturing;

public readonly record struct ItemCapture(IEntitySource Source, Vector2 Position, int Type, int Stack, int Prefix);

public sealed class ItemCapturing : ModSystem
{
	private static readonly Stack<List<ItemCapture>> listStack = new();
	private static readonly Ref<uint> skipCounter = new();

	public override void Load()
	{
		On_Item.NewItem_IEntitySource_int_int_int_int_int_int_bool_int_bool_bool += NewItemDetour;
	}

	public static CaptureHandle<ItemCapture> Capture(List<ItemCapture> captures)
		=> new(listStack, captures);

	public static CaptureHandle<ItemCapture> Capture(out List<ItemCapture> captures)
		=> Capture(captures = new());

	public static CounterHandle Suspend()
		=> new(skipCounter);

	private static int NewItemDetour(On_Item.orig_NewItem_IEntitySource_int_int_int_int_int_int_bool_int_bool_bool orig, IEntitySource source, int x, int y, int width, int height, int type, int stack, bool noBroadcast, int prefix, bool noGrabDelay, bool reverseLookup)
	{
		if (listStack.TryPeek(out var list) && skipCounter.Value == 0) {
			list.Add(new ItemCapture(source, Main.rand.NextVector2(x, y, x + width, y + height), type, stack, prefix));

			return Main.maxItems;
		}

		return orig(source, x, y, width, height, type, stack, noBroadcast, prefix, noGrabDelay, reverseLookup);
	}
}

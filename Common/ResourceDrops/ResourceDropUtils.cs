using System;
using System.Linq;
using Terraria;

namespace TerrariaOverhaul.Common.ResourceDrops
{
	public static class ResourceDropUtils
	{
		public static readonly float DefaultResourceDropRange = 1280f;

		public static int GetDefaultDropCount(Player player, int statCurrent, int statMax, int statGainPerPickup, int? maxDrops = null, int? countedItemId = null, float? countingRangeOverride = null)
		{
			int neededHealth = statMax - statCurrent;
			int neededDrops = (int)Math.Ceiling(neededHealth / (float)statGainPerPickup);
			int dropsCount = neededDrops;

			if (countedItemId.HasValue) {
				int id = countedItemId.Value;
				float checkRange = countingRangeOverride ?? DefaultResourceDropRange;
				int existingDrops = Main.item.Count(i => i?.active == true && i.type == id && player.WithinRange(i.position, checkRange));

				dropsCount = Math.Max(0, dropsCount - existingDrops);
			}

			if (maxDrops.HasValue) {
				dropsCount = Math.Min(maxDrops.Value, dropsCount);
			}

			return dropsCount;
		}
	}
}

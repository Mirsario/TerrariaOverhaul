// Copyright (c) 2020-2026 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;
using Hook = TerrariaOverhaul.Common.Hooks.Items.ICanTurnDuringItemUse;

namespace TerrariaOverhaul.Common.Hooks.Items;

internal interface ICanTurnDuringItemUse
{
	public static readonly GlobalHookList<GlobalItem> Hook = ItemLoader.AddModHook(GlobalHookList<GlobalItem>.Create(i => ((Hook)i).CanTurnDuringItemUse));

	bool? CanTurnDuringItemUse(Item item, Player player);

	public static bool Invoke(Item item, Player player)
	{
		bool? globalResult = null;

		foreach (Hook g in Hook.Enumerate(item)) {
			bool? result = g.CanTurnDuringItemUse(item, player);

			if (result.HasValue) {
				if (result.Value) {
					globalResult = true;
				} else {
					return false;
				}
			}
		}

		return globalResult ?? item.useTurn;
	}
}

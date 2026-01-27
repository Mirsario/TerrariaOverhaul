// Copyright (c) 2020-2026 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;
using Hook = TerrariaOverhaul.Common.Charging.ICanStartPowerAttack;

namespace TerrariaOverhaul.Common.Charging;

internal interface ICanStartPowerAttack
{
	public static readonly GlobalHookList<GlobalItem> Hook = ItemLoader.AddModHook(GlobalHookList<GlobalItem>.Create(i => ((Hook)i).CanStartPowerAttack));

	bool CanStartPowerAttack(Item item, Player player);

	public static bool Invoke(Item item, Player player)
	{
		if (item.ModItem is Hook hook && !hook.CanStartPowerAttack(item, player)) {
			return false;
		}

		foreach (Hook g in Hook.Enumerate(item)) {
			if (!g.CanStartPowerAttack(item, player)) {
				return false;
			}
		}

		return true;
	}
}

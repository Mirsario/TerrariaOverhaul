// Copyright (c) 2020-2025 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;
using Hook = TerrariaOverhaul.Common.Hooks.Items.ICanDoMeleeDamage;

namespace TerrariaOverhaul.Common.Hooks.Items;

public interface ICanDoMeleeDamage
{
	public static readonly GlobalHookList<GlobalItem> Hook = ItemLoader.AddModHook(GlobalHookList<GlobalItem>.Create(i => ((Hook)i).CanDoMeleeDamage));

	bool CanDoMeleeDamage(Item item, Player player);

	public static bool Invoke(Item item, Player player)
	{
		foreach (Hook g in Hook.Enumerate(item)) {
			if (!g.CanDoMeleeDamage(item, player)) {
				return false;
			}
		}

		return true;
	}
}

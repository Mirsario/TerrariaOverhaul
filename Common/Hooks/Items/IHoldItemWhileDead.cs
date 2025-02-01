// Copyright (c) 2020-2025 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;
using Hook = TerrariaOverhaul.Common.Hooks.Items.IHoldItemWhileDead;

namespace TerrariaOverhaul.Common.Hooks.Items;

public interface IHoldItemWhileDead
{
	public static readonly GlobalHookList<GlobalItem> Hook = ItemLoader.AddModHook(GlobalHookList<GlobalItem>.Create(i => ((Hook)i).HoldItemWhileDead));

	void HoldItemWhileDead(Item item, Player player);

	public static void Invoke(Item item, Player player)
	{
		foreach (Hook g in Hook.Enumerate(item)) {
			g.HoldItemWhileDead(item, player);
		}
	}
}

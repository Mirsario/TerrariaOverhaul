using System;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;

namespace TerrariaOverhaul.Common.Hooks.Items
{
	public interface IShowItemCrosshair
	{
		public static readonly HookList<GlobalItem, Func<Item, Player, bool>> Hook = ItemLoader.AddModHook(new HookList<GlobalItem, Func<Item, Player, bool>>(
			//Method reference
			typeof(IShowItemCrosshair).GetMethod(nameof(IShowItemCrosshair.ShowItemCrosshair)),
			//Invocation
			e => (Item item, Player player) => {
				foreach(IShowItemCrosshair g in e.Enumerate(item)) {
					if(g.ShowItemCrosshair(item, player)) {
						return true;
					}
				}

				return false;
			}
		));

		bool ShowItemCrosshair(Item item, Player player);
	}
}

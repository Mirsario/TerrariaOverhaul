using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;
using Hook = TerrariaOverhaul.Common.Hooks.Items.IShowItemCrosshair;

namespace TerrariaOverhaul.Common.Hooks.Items
{
	public interface IShowItemCrosshair
	{
		public delegate bool Delegate(Item item, Player player);

		public static readonly HookList<GlobalItem, Delegate> Hook = ItemLoader.AddModHook(new HookList<GlobalItem, Delegate>(
			// Method reference
			typeof(Hook).GetMethod(nameof(ShowItemCrosshair)),
			// Invocation
			e => (Item item, Player player) => {
				foreach (Hook g in e.Enumerate(item)) {
					if (g.ShowItemCrosshair(item, player)) {
						return true;
					}
				}

				return false;
			}
		));

		bool ShowItemCrosshair(Item item, Player player);
	}
}

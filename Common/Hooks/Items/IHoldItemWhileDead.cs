using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;
using Hook = TerrariaOverhaul.Common.Hooks.Items.IHoldItemWhileDead;

namespace TerrariaOverhaul.Common.Hooks.Items
{
	public interface IHoldItemWhileDead
	{
		public delegate void Delegate(Item item, Player player);

		public static readonly HookList<GlobalItem, Delegate> Hook = ItemLoader.AddModHook(new HookList<GlobalItem, Delegate>(
			// Method reference
			typeof(Hook).GetMethod(nameof(HoldItemWhileDead)),
			// Invocation
			e => (Item item, Player player) => {
				foreach (Hook g in e.Enumerate(item)) {
					g.HoldItemWhileDead(item, player);
				}
			}
		));

		void HoldItemWhileDead(Item item, Player player);
	}
}

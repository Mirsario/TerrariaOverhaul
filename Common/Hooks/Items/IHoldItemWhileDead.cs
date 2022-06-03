using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;
using Hook = TerrariaOverhaul.Common.Hooks.Items.IHoldItemWhileDead;

namespace TerrariaOverhaul.Common.Hooks.Items
{
	public interface IHoldItemWhileDead
	{
		public static readonly HookList<GlobalItem> Hook = ItemLoader.AddModHook(new HookList<GlobalItem>(typeof(Hook).GetMethod(nameof(HoldItemWhileDead))));

		void HoldItemWhileDead(Item item, Player player);

		public static void Invoke(Item item, Player player)
		{
			foreach (Hook g in Hook.Enumerate(item)) {
				g.HoldItemWhileDead(item, player);
			}
		}
	}
}

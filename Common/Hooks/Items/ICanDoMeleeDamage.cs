using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;
using Hook = TerrariaOverhaul.Common.Hooks.Items.ICanDoMeleeDamage;

namespace TerrariaOverhaul.Common.Hooks.Items
{
	public interface ICanDoMeleeDamage
	{
		public static readonly HookList<GlobalItem> Hook = ItemLoader.AddModHook(new HookList<GlobalItem>(typeof(Hook).GetMethod(nameof(CanDoMeleeDamage))));

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
}

using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;
using Hook = TerrariaOverhaul.Common.Hooks.Items.IModifyItemMeleeRange;

namespace TerrariaOverhaul.Common.Hooks.Items
{
	public interface IModifyItemMeleeRange
	{
		public delegate void Delegate(Item item, Player player, ref float range);

		public static readonly HookList<GlobalItem, Delegate> Hook = ItemLoader.AddModHook(new HookList<GlobalItem, Delegate>(
			// Method reference
			typeof(Hook).GetMethod(nameof(ModifyMeleeRange)),
			// Invocation
			e => (Item item, Player player, ref float range) => {
				(item.ModItem as Hook)?.ModifyMeleeRange(item, player, ref range);

				foreach (Hook g in e.Enumerate(item)) {
					g.ModifyMeleeRange(item, player, ref range);
				}
			}
		));

		void ModifyMeleeRange(Item item, Player player, ref float range);
	}
}

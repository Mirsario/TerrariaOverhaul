using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;
using Hook = TerrariaOverhaul.Common.Hooks.Items.IModifyItemMeleeRange;

namespace TerrariaOverhaul.Common.Hooks.Items;

public interface IModifyItemMeleeRange
{
	public static readonly HookList<GlobalItem> Hook = ItemLoader.AddModHook(new HookList<GlobalItem>(typeof(Hook).GetMethod(nameof(ModifyMeleeRange))));

	void ModifyMeleeRange(Item item, Player player, ref float range);

	public static void Invoke(Item item, Player player, ref float range)
	{
		(item.ModItem as Hook)?.ModifyMeleeRange(item, player, ref range);

		foreach (Hook g in Hook.Enumerate(item)) {
			g.ModifyMeleeRange(item, player, ref range);
		}
	}
}

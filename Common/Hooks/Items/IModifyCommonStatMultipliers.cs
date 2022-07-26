using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;
using TerrariaOverhaul.Utilities;
using Hook = TerrariaOverhaul.Common.Hooks.Items.IModifyCommonStatMultipliers;

namespace TerrariaOverhaul.Common.Hooks.Items;

public interface IModifyCommonStatMultipliers
{
	public static readonly HookList<GlobalItem> Hook = ItemLoader.AddModHook(new HookList<GlobalItem>(typeof(Hook).GetMethod(nameof(ModifyCommonStatMultipliers))));

	void ModifyCommonStatMultipliers(Item item, Player player, ref CommonStatMultipliers multipliers);

	public static CommonStatMultipliers GetMultipliers(Item item, Player player)
	{
		var multipliers = CommonStatMultipliers.Default;

		Invoke(item, player, ref multipliers);

		return multipliers;
	}

	public static void Invoke(Item item, Player player, ref CommonStatMultipliers multipliers)
	{
		(item.ModItem as Hook)?.ModifyCommonStatMultipliers(item, player, ref multipliers);
		
		foreach (Hook g in Hook.Enumerate(item)) {
			g.ModifyCommonStatMultipliers(item, player, ref multipliers);
		}
	}
}

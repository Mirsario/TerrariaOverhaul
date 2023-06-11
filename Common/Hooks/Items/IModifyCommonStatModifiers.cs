using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;
using TerrariaOverhaul.Utilities;
using Hook = TerrariaOverhaul.Common.Hooks.Items.IModifyCommonStatModifiers;

namespace TerrariaOverhaul.Common.Hooks.Items;

public interface IModifyCommonStatModifiers
{
	public static readonly HookList<GlobalItem> Hook = ItemLoader.AddModHook(new HookList<GlobalItem>(typeof(Hook).GetMethod(nameof(ModifyCommonStatMultipliers))));

	void ModifyCommonStatMultipliers(Item item, Player player, ref CommonStatModifiers multipliers);

	public static CommonStatModifiers GetMultipliers(Item item, Player player)
	{
		var multipliers = new CommonStatModifiers();

		Invoke(item, player, ref multipliers);

		return multipliers;
	}

	public static void Invoke(Item item, Player player, ref CommonStatModifiers multipliers)
	{
		(item.ModItem as Hook)?.ModifyCommonStatMultipliers(item, player, ref multipliers);
		
		foreach (Hook g in Hook.Enumerate(item)) {
			g.ModifyCommonStatMultipliers(item, player, ref multipliers);
		}
	}
}

using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;
using Hook = TerrariaOverhaul.Common.Charging.ICanStartPowerAttack;

namespace TerrariaOverhaul.Common.Charging;

public interface ICanStartPowerAttack
{
	public static readonly HookList<GlobalItem> Hook = ItemLoader.AddModHook(new HookList<GlobalItem>(typeof(Hook).GetMethod(nameof(CanStartPowerAttack))));

	bool CanStartPowerAttack(Item item, Player player);

	public static bool Invoke(Item item, Player player)
	{
		if (item.ModItem is Hook hook && !hook.CanStartPowerAttack(item, player)) {
			return false;
		}

		foreach (Hook g in Hook.Enumerate(item)) {
			if (!g.CanStartPowerAttack(item, player)) {
				return false;
			}
		}

		return true;
	}
}

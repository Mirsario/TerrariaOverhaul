using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;
using TerrariaOverhaul.Utilities;
using Hook = TerrariaOverhaul.Common.Hooks.Items.IModifyCommonStatMultipliers;

namespace TerrariaOverhaul.Common.Hooks.Items
{
	public interface IModifyCommonStatMultipliers
	{
		public delegate void Delegate(Item item, Player player, ref CommonStatMultipliers multipliers);

		public static readonly HookList<GlobalItem, Delegate> Hook = ItemLoader.AddModHook(new HookList<GlobalItem, Delegate>(
			// Method reference
			typeof(Hook).GetMethod(nameof(ModifyCommonStatMultipliers)),
			// Invocation
			e => (Item item, Player player, ref CommonStatMultipliers multipliers) => {
				(item.ModItem as Hook)?.ModifyCommonStatMultipliers(item, player, ref multipliers);

				foreach (Hook g in e.Enumerate(item)) {
					g.ModifyCommonStatMultipliers(item, player, ref multipliers);
				}
			}
		));

		void ModifyCommonStatMultipliers(Item item, Player player, ref CommonStatMultipliers multipliers);

		public static CommonStatMultipliers GetMultipliers(Item item, Player player)
		{
			var multipliers = CommonStatMultipliers.Default;

			Hook.Invoke(item, player, ref multipliers);

			return multipliers;
		}
	}
}

using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;
using Hook = TerrariaOverhaul.Common.Hooks.Items.ICanMeleeCollideWithNPC;

namespace TerrariaOverhaul.Common.Hooks.Items
{
	public interface ICanMeleeCollideWithNPC
	{
		public static readonly HookList<GlobalItem> Hook = ItemLoader.AddModHook(new HookList<GlobalItem>(typeof(Hook).GetMethod(nameof(CanMeleeCollideWithNPC))));

		bool? CanMeleeCollideWithNPC(Item item, Player player, NPC target);

		public static bool? Invoke(Item item, Player player, NPC target)
		{
			bool? globalResult = null;

			foreach (Hook g in Hook.Enumerate(item)) {
				bool? result = g.CanMeleeCollideWithNPC(item, player, target);

				if (result.HasValue) {
					if (result.Value) {
						globalResult = true;
					} else {
						return false;
					}
				}
			}

			return globalResult;
		}
	}
}

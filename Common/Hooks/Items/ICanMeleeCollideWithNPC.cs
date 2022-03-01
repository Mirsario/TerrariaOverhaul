using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;
using Hook = TerrariaOverhaul.Common.Hooks.Items.ICanMeleeCollideWithNPC;

namespace TerrariaOverhaul.Common.Hooks.Items
{
	public interface ICanMeleeCollideWithNPC
	{
		public delegate bool? Delegate(Item item, Player player, NPC target);

		public static readonly HookList<GlobalItem, Delegate> Hook = ItemLoader.AddModHook(new HookList<GlobalItem, Delegate>(
			// Method reference
			typeof(Hook).GetMethod(nameof(CanMeleeCollideWithNPC)),
			// Invocation
			e => (Item item, Player player, NPC target) => {
				bool? globalResult = null;

				foreach (Hook g in e.Enumerate(item)) {
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
		));

		bool? CanMeleeCollideWithNPC(Item item, Player player, NPC target);
	}
}

using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;

namespace TerrariaOverhaul.Common.Hooks.Items
{
	public interface ICanTurnDuringItemUse
	{
		public delegate bool Delegate(Item item, Player player);

		public static readonly HookList<GlobalItem, Delegate> Hook = ItemLoader.AddModHook(new HookList<GlobalItem, Delegate>(
			//Method reference
			typeof(ICanTurnDuringItemUse).GetMethod(nameof(ICanTurnDuringItemUse.CanTurnDuringItemUse)),
			//Invocation
			e => (Item item, Player player) => {
				bool? globalResult = null;

				foreach (ICanTurnDuringItemUse g in e.Enumerate(item)) {
					bool? result = g.CanTurnDuringItemUse(item, player);

					if (result.HasValue) {
						if (result.Value) {
							globalResult = true;
						} else {
							return false;
						}
					}
				}

				return globalResult ?? item.useTurn;
			}
		));

		bool? CanTurnDuringItemUse(Item item, Player player);
	}
}

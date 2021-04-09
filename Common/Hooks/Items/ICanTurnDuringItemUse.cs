using System;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;

namespace TerrariaOverhaul.Common.Hooks.Items
{
	public interface ICanTurnDuringItemUse
	{
		bool? CanTurnDuringItemUse(Item item, Player player);
	}

	//TODO: This class will not be needed with C# 8.0 default interface implementations.
	public sealed class HookCanTurnDuringItemUse : ILoadable
	{
		public static HookList<GlobalItem, Func<Item, Player, bool>> Hook { get; private set; } = new HookList<GlobalItem, Func<Item, Player, bool>>(
			//Method reference
			typeof(ICanTurnDuringItemUse).GetMethod(nameof(ICanTurnDuringItemUse.CanTurnDuringItemUse)),
			//Invocation
			e => (Item item, Player player) => {
				bool? globalResult = null;

				foreach(ICanTurnDuringItemUse g in e.Enumerate(item)) {
					bool? result = g.CanTurnDuringItemUse(item, player);

					if(result.HasValue) {
						if(result.Value) {
							globalResult = true;
						} else {
							return false;
						}
					}
				}

				return globalResult ?? item.useTurn;
			}
		);

		public void Load(Mod mod) => ItemLoader.AddModHook(Hook);
		public void Unload() { }
	}
}

using System;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;

namespace TerrariaOverhaul.Common.Hooks.Items
{
	public interface IShowItemCrosshair
	{
		bool ShowItemCrosshair(Item item, Player player);
	}

	//TODO: This class will not be needed with C# 8.0 default interface implementations.
	public sealed class HookShowItemCrosshair : ILoadable
	{
		public static HookList<GlobalItem, Func<Item, Player, bool>> Hook { get; private set; } = new HookList<GlobalItem, Func<Item, Player, bool>>(
			//Method reference
			typeof(IShowItemCrosshair).GetMethod(nameof(IShowItemCrosshair.ShowItemCrosshair)),
			//Invocation
			e => (Item item, Player player) => {
				foreach(IShowItemCrosshair g in e.Enumerate(item)) {
					if(g.ShowItemCrosshair(item, player)) {
						return true;
					}
				}

				return false;
			}
		);

		public void Load(Mod mod) => ItemLoader.AddModHook(Hook);
		public void Unload() { }
	}
}

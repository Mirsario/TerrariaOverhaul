using System;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;

namespace TerrariaOverhaul.Common.Hooks.Items
{
	public interface IHoldItemWhileDead
	{
		void HoldItemWhileDead(Item item, Player player);
	}

	//TODO: This class will not be needed with C# 8.0 default interface implementations.
	public sealed class HookHoldItemWhileDead : ILoadable
	{
		public static HookList<GlobalItem, Action<Item, Player>> Hook { get; private set; } = new HookList<GlobalItem, Action<Item, Player>>(
			//Method reference
			typeof(IHoldItemWhileDead).GetMethod(nameof(IHoldItemWhileDead.HoldItemWhileDead)),
			//Invocation
			e => (Item item, Player player) => {
				foreach(IHoldItemWhileDead g in e.Enumerate(item)) {
					g.HoldItemWhileDead(item, player);
				}
			}
		);

		public void Load(Mod mod) => ItemLoader.AddModHook(Hook);
		public void Unload() { }
	}

	public sealed class PlayerHoldItemWhileDeadHookImplementation : ModPlayer
	{
		public override void UpdateDead()
		{
			var heldItem = Player.HeldItem;

			if(heldItem?.IsAir == false) {
				HookHoldItemWhileDead.Hook.Invoke(heldItem, Player);
			}
		}
	}
}

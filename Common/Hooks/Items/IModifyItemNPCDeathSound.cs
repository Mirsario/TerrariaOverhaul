using System;
using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;

namespace TerrariaOverhaul.Common.Hooks.Items
{
	public interface IModifyItemNPCDeathSound
	{
		void ModifyItemNPCDeathSound(Item item, Player player, NPC target, ref SoundStyle customDeathSound, ref bool playNPCDeathSound);
	}

	//TODO: This class will not be needed with C# 8.0 default interface implementations.
	public sealed class HookModifyItemNPCDeathSound : ILoadable
	{
		public delegate void Delegate(Item item, Player player, NPC target, ref SoundStyle customDeathSound, ref bool playNPCDeathSound);

		public static HookList<GlobalItem, Delegate> Hook { get; private set; } = new HookList<GlobalItem, Delegate>(
			//Method reference
			typeof(IModifyItemNPCDeathSound).GetMethod(nameof(IModifyItemNPCDeathSound.ModifyItemNPCDeathSound)),
			//Invocation
			e => (Item item, Player player, NPC target, ref SoundStyle customDeathSound, ref bool playNPCDeathSound) => {
				foreach(IModifyItemNPCDeathSound g in e.Enumerate(item)) {
					g.ModifyItemNPCDeathSound(item, player, target, ref customDeathSound, ref playNPCDeathSound);
				}
			}
		);

		public void Load(Mod mod) => ItemLoader.AddModHook(Hook);
		public void Unload() { }
	}
}

using System;
using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;

namespace TerrariaOverhaul.Common.Hooks.Items
{
	public interface IModifyItemNPCDeathSound
	{
		public delegate void Delegate(Item item, Player player, NPC target, ref SoundStyle customDeathSound, ref bool playNPCDeathSound);

		public static readonly HookList<GlobalItem, Delegate> Hook = ItemLoader.AddModHook(new HookList<GlobalItem, Delegate>(
			//Method reference
			typeof(IModifyItemNPCDeathSound).GetMethod(nameof(IModifyItemNPCDeathSound.ModifyItemNPCDeathSound)),
			//Invocation
			e => (Item item, Player player, NPC target, ref SoundStyle customDeathSound, ref bool playNPCDeathSound) => {
				foreach(IModifyItemNPCDeathSound g in e.Enumerate(item)) {
					g.ModifyItemNPCDeathSound(item, player, target, ref customDeathSound, ref playNPCDeathSound);
				}
			}
		));

		void ModifyItemNPCDeathSound(Item item, Player player, NPC target, ref SoundStyle customDeathSound, ref bool playNPCDeathSound);
	}
}

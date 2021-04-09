using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;

namespace TerrariaOverhaul.Common.Hooks.Items
{
	public interface IModifyItemNPCHitSound
	{
		void ModifyItemNPCHitSound(Item item, Player player, NPC target, ref SoundStyle customHitSound, ref bool playNPCHitSound);
	}

	//TODO: This class will not be needed with C# 8.0 default interface implementations.
	public sealed class HookModifyItemNPCHitSound : ILoadable
	{
		public delegate void Delegate(Item item, Player player, NPC target, ref SoundStyle customHitSound, ref bool playNPCHitSound);

		public static HookList<GlobalItem, Delegate> Hook { get; private set; } = new HookList<GlobalItem, Delegate>(
			//Method reference
			typeof(IModifyItemNPCHitSound).GetMethod(nameof(IModifyItemNPCHitSound.ModifyItemNPCHitSound)),
			//Invocation
			e => (Item item, Player player, NPC target, ref SoundStyle customHitSound, ref bool playNPCHitSound) => {
				foreach(IModifyItemNPCHitSound g in e.Enumerate(item)) {
					g.ModifyItemNPCHitSound(item, player, target, ref customHitSound, ref playNPCHitSound);
				}
			}
		);

		public void Load(Mod mod) => ItemLoader.AddModHook(Hook);
		public void Unload() { }
	}
}

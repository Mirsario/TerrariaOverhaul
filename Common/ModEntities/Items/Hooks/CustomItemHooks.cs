using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;

namespace TerrariaOverhaul.Common.ModEntities.Items.Hooks
{
	//TODO: This class will not be needed with C# 8.0 default interface implementations.
	public sealed class CustomItemHooks : ILoadable
	{
		public delegate void DelegateModifyItemNPCHitSound(Item item, Player player, NPC target, ref SoundStyle customHitSound, ref bool playNPCHitSound);
		public delegate void DelegateModifyItemNPCDeathSound(Item item, Player player, NPC target, ref SoundStyle customDeathSound, ref bool playNPCDeathSound);

		public static HookList<GlobalItem, DelegateModifyItemNPCHitSound> ModifyItemNPCHitSound { get; private set; } = new HookList<GlobalItem, DelegateModifyItemNPCHitSound>(
			//Method reference
			typeof(IModifyItemNPCHitSound).GetMethod(nameof(IModifyItemNPCHitSound.ModifyItemNPCHitSound)),
			//Invocation
			e => (Item item, Player player, NPC target, ref SoundStyle customHitSound, ref bool playNPCHitSound) => {
				foreach(IModifyItemNPCHitSound g in ModifyItemNPCHitSound.Enumerate(item)) {
					g.ModifyItemNPCHitSound(item, player, target, ref customHitSound, ref playNPCHitSound);
				}
			}
		);
		public static HookList<GlobalItem, DelegateModifyItemNPCDeathSound> ModifyItemNPCDeathSound { get; private set; } = new HookList<GlobalItem, DelegateModifyItemNPCDeathSound>(
			//Method reference
			typeof(IModifyItemNPCDeathSound).GetMethod(nameof(IModifyItemNPCDeathSound.ModifyItemNPCDeathSound)),
			//Invocation
			e => (Item item, Player player, NPC target, ref SoundStyle customDeathSound, ref bool playNPCDeathSound) => {
				foreach(IModifyItemNPCDeathSound g in ModifyItemNPCDeathSound.Enumerate(item)) {
					g.ModifyItemNPCDeathSound(item, player, target, ref customDeathSound, ref playNPCDeathSound);
				}
			}
		);

		//TODO: AddModHook will be able to be used in a static constructor when the .NET Core tML branch is merged, and assembly unloading is implemented.
		public void Load(Mod mod)
		{
			ItemLoader.AddModHook(ModifyItemNPCHitSound);
			ItemLoader.AddModHook(ModifyItemNPCDeathSound);
		}
		public void Unload() { }
	}
}

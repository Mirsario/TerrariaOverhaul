using System;
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
				foreach(IModifyItemNPCHitSound g in e.Enumerate(item)) {
					g.ModifyItemNPCHitSound(item, player, target, ref customHitSound, ref playNPCHitSound);
				}
			}
		);
		public static HookList<GlobalItem, DelegateModifyItemNPCDeathSound> ModifyItemNPCDeathSound { get; private set; } = new HookList<GlobalItem, DelegateModifyItemNPCDeathSound>(
			//Method reference
			typeof(IModifyItemNPCDeathSound).GetMethod(nameof(IModifyItemNPCDeathSound.ModifyItemNPCDeathSound)),
			//Invocation
			e => (Item item, Player player, NPC target, ref SoundStyle customDeathSound, ref bool playNPCDeathSound) => {
				foreach(IModifyItemNPCDeathSound g in e.Enumerate(item)) {
					g.ModifyItemNPCDeathSound(item, player, target, ref customDeathSound, ref playNPCDeathSound);
				}
			}
		);
		public static HookList<GlobalItem, Func<Item, Player, bool>> CanTurnDuringItemUse { get; private set; } = new HookList<GlobalItem, Func<Item, Player, bool>>(
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

		//TODO: AddModHook will be able to be used in a static constructor when the .NET Core tML branch is merged, and assembly unloading is implemented.
		public void Load(Mod mod)
		{
			ItemLoader.AddModHook(ModifyItemNPCHitSound);
			ItemLoader.AddModHook(ModifyItemNPCDeathSound);
			ItemLoader.AddModHook(CanTurnDuringItemUse);
		}
		public void Unload() { }
	}
}

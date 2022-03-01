using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;
using Hook = TerrariaOverhaul.Common.Hooks.Items.IModifyItemNPCHitSound;

namespace TerrariaOverhaul.Common.Hooks.Items
{
	public interface IModifyItemNPCHitSound
	{
		public delegate void Delegate(Item item, Player player, NPC target, ref ISoundStyle customHitSound, ref bool playNPCHitSound);

		public static readonly HookList<GlobalItem, Delegate> Hook = ItemLoader.AddModHook(new HookList<GlobalItem, Delegate>(
			// Method reference
			typeof(Hook).GetMethod(nameof(ModifyItemNPCHitSound)),
			// Invocation
			e => (Item item, Player player, NPC target, ref ISoundStyle customHitSound, ref bool playNPCHitSound) => {
				foreach (Hook g in e.Enumerate(item)) {
					g.ModifyItemNPCHitSound(item, player, target, ref customHitSound, ref playNPCHitSound);
				}
			}
		));

		void ModifyItemNPCHitSound(Item item, Player player, NPC target, ref ISoundStyle customHitSound, ref bool playNPCHitSound);
	}
}

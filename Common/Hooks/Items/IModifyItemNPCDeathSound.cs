using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;
using Hook = TerrariaOverhaul.Common.Hooks.Items.IModifyItemNPCDeathSound;

namespace TerrariaOverhaul.Common.Hooks.Items
{
	public interface IModifyItemNPCDeathSound
	{
		public delegate void Delegate(Item item, Player player, NPC target, ref ISoundStyle customDeathSound, ref bool playNPCDeathSound);

		public static readonly HookList<GlobalItem, Delegate> Hook = ItemLoader.AddModHook(new HookList<GlobalItem, Delegate>(
			// Method reference
			typeof(Hook).GetMethod(nameof(ModifyItemNPCDeathSound)),
			// Invocation
			e => (Item item, Player player, NPC target, ref ISoundStyle customDeathSound, ref bool playNPCDeathSound) => {
				foreach (Hook g in e.Enumerate(item)) {
					g.ModifyItemNPCDeathSound(item, player, target, ref customDeathSound, ref playNPCDeathSound);
				}
			}
		));

		void ModifyItemNPCDeathSound(Item item, Player player, NPC target, ref ISoundStyle customDeathSound, ref bool playNPCDeathSound);
	}
}

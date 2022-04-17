using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;
using Hook = TerrariaOverhaul.Common.Hooks.Items.IModifyItemUseSound;

namespace TerrariaOverhaul.Common.Hooks.Items
{
	public interface IModifyItemUseSound
	{
		public delegate void Delegate(Item item, Player player, ref ISoundStyle? useSound);

		public static readonly HookList<GlobalItem, Delegate> Hook = ItemLoader.AddModHook(new HookList<GlobalItem, Delegate>(
			// Method reference
			typeof(Hook).GetMethod(nameof(ModifyItemUseSound)),
			// Invocation
			e => (Item item, Player player, ref ISoundStyle? useSound) => {
				(item.ModItem as Hook)?.ModifyItemUseSound(item, player, ref useSound);

				foreach (Hook g in e.Enumerate(item)) {
					g.ModifyItemUseSound(item, player, ref useSound);
				}
			}
		));

		void ModifyItemUseSound(Item item, Player player, ref ISoundStyle? useSound);
	}
}

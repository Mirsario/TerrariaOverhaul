using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;
using Hook = TerrariaOverhaul.Common.Hooks.Items.IModifyItemUseSound;

namespace TerrariaOverhaul.Common.Hooks.Items;

public interface IModifyItemUseSound
{
	public static readonly GlobalHookList<GlobalItem> Hook = ItemLoader.AddModHook(new GlobalHookList<GlobalItem>(typeof(Hook).GetMethod(nameof(ModifyItemUseSound))));

	void ModifyItemUseSound(Item item, Player player, ref SoundStyle? useSound);

	public static void Invoke(Item item, Player player, ref SoundStyle? useSound)
	{
		(item.ModItem as Hook)?.ModifyItemUseSound(item, player, ref useSound);

		foreach (Hook g in Hook.Enumerate(item)) {
			g.ModifyItemUseSound(item, player, ref useSound);
		}
	}
}

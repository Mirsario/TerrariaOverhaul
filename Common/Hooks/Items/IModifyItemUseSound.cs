// Copyright (c) 2020-2025 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;
using Hook = TerrariaOverhaul.Common.Hooks.Items.IModifyItemUseSound;

namespace TerrariaOverhaul.Common.Hooks.Items;

public interface IModifyItemUseSound
{
	public static readonly GlobalHookList<GlobalItem> Hook = ItemLoader.AddModHook(GlobalHookList<GlobalItem>.Create(i => ((Hook)i).ModifyItemUseSound));

	void ModifyItemUseSound(Item item, Player player, ref SoundStyle? useSound);

	public static void Invoke(Item item, Player player, ref SoundStyle? useSound)
	{
		(item.ModItem as Hook)?.ModifyItemUseSound(item, player, ref useSound);

		foreach (Hook g in Hook.Enumerate(item)) {
			g.ModifyItemUseSound(item, player, ref useSound);
		}
	}
}

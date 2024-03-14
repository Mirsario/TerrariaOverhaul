﻿using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;
using Hook = TerrariaOverhaul.Common.Hooks.Items.IModifyItemNPCHitSound;

namespace TerrariaOverhaul.Common.Hooks.Items;

public interface IModifyItemNPCHitSound
{
	public static readonly GlobalHookList<GlobalItem> Hook = ItemLoader.AddModHook(GlobalHookList<GlobalItem>.Create(i => ((Hook)i).ModifyItemNPCHitSound));

	void ModifyItemNPCHitSound(Item item, Player player, NPC target, ref SoundStyle? customHitSound, ref bool playNPCHitSound);

	public static void Invoke(Item item, Player player, NPC target, ref SoundStyle? customHitSound, ref bool playNPCHitSound)
	{
		(item.ModItem as Hook)?.ModifyItemNPCHitSound(item, player, target, ref customHitSound, ref playNPCHitSound);

		foreach (Hook g in Hook.Enumerate(item)) {
			g.ModifyItemNPCHitSound(item, player, target, ref customHitSound, ref playNPCHitSound);
		}
	}
}

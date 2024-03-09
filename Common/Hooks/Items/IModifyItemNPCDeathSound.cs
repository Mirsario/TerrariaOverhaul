using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;
using Hook = TerrariaOverhaul.Common.Hooks.Items.IModifyItemNPCDeathSound;

namespace TerrariaOverhaul.Common.Hooks.Items;

public interface IModifyItemNPCDeathSound
{
	public static readonly GlobalHookList<GlobalItem> Hook = ItemLoader.AddModHook(GlobalHookList<GlobalItem>.Create(i => ((Hook)i).ModifyItemNPCDeathSound));

	void ModifyItemNPCDeathSound(Item item, Player player, NPC target, ref SoundStyle? customDeathSound, ref bool playNPCDeathSound);

	public static void Invoke(Item item, Player player, NPC target, ref SoundStyle? customDeathSound, ref bool playNPCDeathSound)
	{
		foreach (Hook g in Hook.Enumerate(item)) {
			g.ModifyItemNPCDeathSound(item, player, target, ref customDeathSound, ref playNPCDeathSound);
		}
	}
}

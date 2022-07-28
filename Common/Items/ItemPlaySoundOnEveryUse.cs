using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Core.ItemComponents;

namespace TerrariaOverhaul.Common.Items;

[Autoload(Side = ModSide.Client)]
public sealed class ItemPlaySoundOnEveryUse : ItemComponent
{
	public override bool? UseItem(Item item, Player player)
	{
		if (Enabled) {
			ItemID.Sets.SkipsInitialUseSound[item.type] = true;

			if (item.UseSound.HasValue) {
				SoundEngine.PlaySound(item.UseSound.Value, player.Center);
			}
		}

		return base.UseItem(item, player);
	}
}

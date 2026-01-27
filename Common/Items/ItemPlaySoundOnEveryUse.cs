// Copyright (c) 2020-2026 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Core.ItemComponents;

namespace TerrariaOverhaul.Common.Items;

[Autoload(Side = ModSide.Client)]
internal sealed class ItemPlaySoundOnEveryUse : ItemComponent
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

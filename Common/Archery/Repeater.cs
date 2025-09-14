// Copyright (c) 2020-2025 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using Terraria;
using Terraria.ID;
using TerrariaOverhaul.Core.ItemOverhauls;

namespace TerrariaOverhaul.Common.Archery;

internal partial class Repeater : ItemOverhaul
{
	public override bool ShouldApplyItemOverhaul(Item item)
	{
		return ArcheryWeapons.IsArcheryWeapon(item, out var kind) && kind == ArcheryWeapons.Kind.Repeater;
	}

	public override void SetDefaults(Item item)
	{
		base.SetDefaults(item);

		if (item.UseSound == SoundID.Item5) {
			item.UseSound = ArcheryWeapons.FireSound;
		}
	}
}

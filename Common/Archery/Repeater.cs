using Terraria;
using Terraria.ID;
using TerrariaOverhaul.Core.ItemOverhauls;

namespace TerrariaOverhaul.Common.Archery;

public partial class Repeater : ItemOverhaul
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

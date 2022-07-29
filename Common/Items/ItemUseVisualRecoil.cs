using Terraria;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.PlayerAnimations;
using TerrariaOverhaul.Core.ItemComponents;

namespace TerrariaOverhaul.Common.Items;

[Autoload(Side = ModSide.Client)]
public sealed class ItemUseVisualRecoil : ItemComponent
{
	public float Power { get; set; } = 1f;

	public override bool? UseItem(Item item, Player player)
	{
		if (Enabled && Power != 0f) {
			player.GetModPlayer<PlayerHoldOutAnimation>().VisualRecoil += Power;
		}

		return base.UseItem(item, player);
	}
}

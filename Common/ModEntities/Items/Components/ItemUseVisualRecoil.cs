using Terraria;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.ModEntities.Players.Rendering;
using TerrariaOverhaul.Core.ItemComponents;

namespace TerrariaOverhaul.Common.ModEntities.Items.Components
{
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
}

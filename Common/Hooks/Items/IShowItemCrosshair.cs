using Terraria;

namespace TerrariaOverhaul.Common.Hooks.Items
{
	public interface IShowItemCrosshair
	{
		bool ShowItemCrosshair(Item item, Player player);
	}
}

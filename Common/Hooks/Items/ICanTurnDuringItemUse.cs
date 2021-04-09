using Terraria;

namespace TerrariaOverhaul.Common.Hooks.Items
{
	public interface ICanTurnDuringItemUse
	{
		bool? CanTurnDuringItemUse(Item item, Player player);
	}
}

using Terraria;

namespace TerrariaOverhaul.Common.ModEntities.Items.Hooks
{
	public interface ICanTurnDuringItemUse
	{
		bool? CanTurnDuringItemUse(Item item, Player player);
	}
}

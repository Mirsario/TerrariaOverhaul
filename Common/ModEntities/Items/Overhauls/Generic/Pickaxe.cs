using Terraria;
using Terraria.ID;

namespace TerrariaOverhaul.Common.ModEntities.Items.Overhauls.Generic
{
	public class Pickaxe : MeleeWeapon
	{
		public override bool ShouldApplyItemOverhaul(Item item)
		{
			//Must have mining capabilities
			if(item.pick <= 0) {
				return false;
			}

			//Avoid hammers and placeables
			if(item.hammer > 0 || item.createTile >= TileID.Dirt || item.createWall >= 0) {
				return false;
			}

			//Pickaxes always swing, deal melee damage, don't have channeling, and are visible
			if(item.useStyle != ItemUseStyleID.Swing || item.noMelee || item.channel || item.noUseGraphic) {
				return false;
			}

			return true;
		}
	}
}

using Terraria;
using Terraria.ID;

namespace TerrariaOverhaul.Common.ModEntities.Items.Overhauls.Generic
{
	public class Axe : MeleeWeapon
	{
		public override bool ShouldApplyItemOverhaul(Item item)
		{
			//Must have woodcutting capabilities
			if(item.axe <= 0) {
				return false;
			}

			//Avoid pickaxes and placeables
			if(item.pick > 0 || item.createTile >= TileID.Dirt || item.createWall >= WallID.None) {
				return false;
			}

			//Axes always swing, deal melee damage, don't have channeling, and are visible
			if(item.useStyle != ItemUseStyleID.Swing || item.noMelee || item.channel || item.noUseGraphic) {
				return false;
			}

			return true;
		}
	}
}

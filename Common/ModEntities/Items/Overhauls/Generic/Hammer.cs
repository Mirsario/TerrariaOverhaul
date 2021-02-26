using Terraria;
using Terraria.ID;

namespace TerrariaOverhaul.Common.ModEntities.Items.Overhauls.Generic
{
	public class Hammer : MeleeWeapon
	{
		public override bool ShouldApplyItemOverhaul(Item item)
		{
			//Must have hammering capabilities
			if(item.hammer <= 0) {
				return false;
			}

			//Avoid pickaxes, axes, and placeables
			if(item.pick > 0 || item.axe > 0 || item.createTile >= TileID.Dirt || item.createWall >= 0) {
				return false;
			}

			//Hammers always swing, deal melee damage, don't have channeling, and are visible
			if(item.useStyle != ItemUseStyleID.Swing || item.noMelee || item.channel || item.noUseGraphic) {
				return false;
			}

			return true;
		}
	}
}

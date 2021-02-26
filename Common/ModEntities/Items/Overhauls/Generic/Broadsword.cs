using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace TerrariaOverhaul.Common.ModEntities.Items.Overhauls.Generic
{
	public class Broadsword : MeleeWeapon
	{
		public override bool ShouldApplyItemOverhaul(Item item)
		{
			//Broadswords always swing, deal melee damage, don't have channeling, and are visible
			if(item.useStyle != ItemUseStyleID.Swing || item.noMelee || item.channel || item.noUseGraphic) {
				return false;
			}

			//Avoid tools and blocks
			if(item.pick > 0 || item.axe > 0 || item.hammer > 0 || item.createTile >= TileID.Dirt || item.createWall >= 0) {
				return false;
			}

			if(item.DamageType != DamageClass.Melee) {
				return false;
			}

			return true;
		}
	}
}

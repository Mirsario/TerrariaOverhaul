using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.ItemAnimations;
using TerrariaOverhaul.Common.ModEntities.Items.Components;
using TerrariaOverhaul.Common.ModEntities.Items.Components.Melee;

namespace TerrariaOverhaul.Common.ModEntities.Items.Overhauls
{
	public class Axe : ItemOverhaul
	{
		public override bool ShouldApplyItemOverhaul(Item item)
		{
			// Must have woodcutting capabilities
			if (item.axe <= 0) {
				return false;
			}

			// Avoid pickaxes and placeables
			if (item.pick > 0 || item.createTile >= TileID.Dirt || item.createWall >= WallID.None) {
				return false;
			}

			// Axes always swing, deal melee damage, don't have channeling, and are visible
			if (item.useStyle != ItemUseStyleID.Swing || item.noMelee || item.channel || item.noUseGraphic) {
				return false;
			}

			return true;
		}

		public override void SetDefaults(Item item)
		{
			item.EnableComponent<ItemMeleeGoreInteraction>();
			item.EnableComponent<ItemMeleeAirCombat>();
			item.EnableComponent<ItemMeleeNpcStuns>();
			item.EnableComponent<ItemMeleeCooldownDisabler>();
			item.EnableComponent<ItemMeleeAttackAiming>();

			item.EnableComponent<ItemPlayerAnimator>(c => {
				c.Animation = ModContent.GetInstance<GenericMeleeAnimation>();
			});
		}
	}
}

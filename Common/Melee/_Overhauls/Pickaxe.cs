using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using TerrariaOverhaul.Common.BloodAndGore;
using TerrariaOverhaul.Core.ItemComponents;
using TerrariaOverhaul.Core.ItemOverhauls;

namespace TerrariaOverhaul.Common.Melee;

public class Pickaxe : ItemOverhaul
{
	public static readonly SoundStyle PickaxeNormalSwingSound = new($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Items/Melee/CuttingSwingMediumAlt", 3) {
		Volume = 0.4f,
		PitchVariance = 0.1f,
	};
	
	public override bool ShouldApplyItemOverhaul(Item item)
	{
		// Must have mining capabilities
		if (item.pick <= 0) {
			return false;
		}

		// Avoid hammers and placeables
		if (item.hammer > 0 || item.createTile >= TileID.Dirt || item.createWall >= 0) {
			return false;
		}

		// Pickaxes always swing, deal melee damage, don't have channeling, and are visible
		if (item.useStyle != ItemUseStyleID.Swing || item.noMelee || item.channel || item.noUseGraphic) {
			return false;
		}

		return true;
	}

	public override void SetDefaults(Item item)
	{
		// Defaults

		if (item.UseSound.HasValue && !item.UseSound.Value.IsTheSameAs(SoundID.Item15)) {
			item.UseSound = PickaxeNormalSwingSound;
		}

		// Components

		item.EnableComponent<ItemMeleeGoreInteraction>();
		item.EnableComponent<ItemMeleeNpcStuns>();
		item.EnableComponent<ItemMeleeCooldownReplacement>();
		item.EnableComponent<ItemMeleeAttackAiming>();
		item.EnableComponent<ItemVelocityBasedDamage>();

		if (ItemMeleeSwingVelocity.EnableSwingVelocity) {
			item.EnableComponent<ItemMeleeSwingVelocity>(c => {
				c.DashVelocity = new Vector2(0.5f, 0.5f);
				c.MaxDashVelocity = new Vector2(0f, 5f);

				c.AddVelocityModifier(in ItemMeleeSwingVelocity.Modifiers.DisableVerticalDashesForNonChargedAttacks);
				c.AddVelocityModifier(in ItemMeleeSwingVelocity.Modifiers.DisableUpwardsDashesWhenFalling);
				c.AddVelocityModifier(in ItemMeleeSwingVelocity.Modifiers.DisableDashesForNonChargedAttacksWhenStill);
			});
		}

		// Animation
		item.EnableComponent<GenericMeleeAnimation>();
	}
}

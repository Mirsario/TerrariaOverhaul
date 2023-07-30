using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using TerrariaOverhaul.Common.Charging;
using TerrariaOverhaul.Common.Damage;
using TerrariaOverhaul.Common.Interaction;
using TerrariaOverhaul.Core.Configuration;
using TerrariaOverhaul.Core.ItemComponents;
using TerrariaOverhaul.Core.ItemOverhauls;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.Melee;

public class Axe : ItemOverhaul
{
	public static readonly SoundStyle AxeNormalSwingSound = new($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Items/Melee/CuttingSwingMediumAlt", 3) {
		Volume = 0.4f,
		PitchVariance = 0.1f,
	};
	public static readonly SoundStyle AxeChargedSwingSound = new($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Items/Melee/CuttingSwingHeavy", 2) {
		PitchVariance = 0.1f,
	};

	public static readonly ConfigEntry<bool> EnableAxePowerAttacks = new(ConfigSide.Both, "Melee", nameof(EnableAxePowerAttacks), () => true);

	public override bool ShouldApplyItemOverhaul(Item item)
	{
		// Must have woodcutting capabilities
		if (item.axe <= 0) {
			return false;
		}

		// Avoid pickaxes
		if (item.pick > 0) {
			return false;
		}

		// Avoid placeables
		//if (item.createTile >= TileID.Dirt || item.createWall >= WallID.None) {
		//	return false;
		//}

		// Axes always swing, deal melee damage, don't have channeling, and are visible
		if (item.useStyle != ItemUseStyleID.Swing || item.noMelee || item.channel || item.noUseGraphic) {
			return false;
		}

		return true;
	}

	public override void SetDefaults(Item item)
	{
		// Defaults

		if (item.UseSound.HasValue && !item.UseSound.Value.IsTheSameAs(SoundID.Item15)) {
			item.UseSound = AxeNormalSwingSound;
		}

		// Components

		if (ItemMeleeAirCombat.EnableAirCombat) {
			item.EnableComponent<ItemMeleeAirCombat>();
		}

		if (ItemMeleeSwingVelocity.EnableSwingVelocity) {
			item.EnableComponent<ItemMeleeSwingVelocity>(c => {
				c.DashVelocity = new Vector2(2.0f, 4.5f);
				c.MaxDashVelocity = new Vector2(0f, 6.0f);

				c.AddVelocityModifier(in ItemMeleeSwingVelocity.Modifiers.PowerAttackBoost);
				c.AddVelocityModifier(in ItemMeleeSwingVelocity.Modifiers.PowerAttackGroundBoost);
				c.AddVelocityModifier(in ItemMeleeSwingVelocity.Modifiers.DisableVerticalDashesForNonChargedAttacks);
				c.AddVelocityModifier(in ItemMeleeSwingVelocity.Modifiers.DisableUpwardsDashesWhenFalling);
				c.AddVelocityModifier(in ItemMeleeSwingVelocity.Modifiers.DisableDashesForNonChargedAttacksWhenStill);
			});
		}

		item.EnableComponent<ItemMeleeGoreInteraction>();
		item.EnableComponent<ItemMeleeCooldownReplacement>();
		item.EnableComponent<ItemMeleeAttackAiming>();
		item.EnableComponent<ItemVelocityBasedDamage>();

		// Animation
		item.EnableComponent<QuickSlashMeleeAnimation>(c => {
			c.FlipAttackEachSwing = true;
			c.AnimateLegs = true;
		});

		// Power Attacks
		if (EnableAxePowerAttacks) {
			item.EnableComponent<ItemMeleePowerAttackEffects>();
			item.EnableComponent<ItemPowerAttacks>(c => {
				c.ChargeLengthMultiplier = 1.5f;

				var statsModifiers = new CommonStatModifiers();

				statsModifiers.MeleeDamageMultiplier = statsModifiers.ProjectileDamageMultiplier = 1.5f;
				statsModifiers.MeleeKnockbackMultiplier = statsModifiers.ProjectileKnockbackMultiplier = 1.5f;
				statsModifiers.MeleeRangeMultiplier = 1.4f;
				statsModifiers.ProjectileSpeedMultiplier = 1.5f;

				c.StatModifiers.Single = statsModifiers;
			});

			if (!Main.dedServ) {
				item.EnableComponent<ItemPowerAttackSounds>(c => {
					c.Sound = AxeChargedSwingSound;
					c.ReplacesUseSound = true;
				});
			}
		}
	}
}

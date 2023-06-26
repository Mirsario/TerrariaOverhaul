using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using TerrariaOverhaul.Common.Charging;
using TerrariaOverhaul.Common.Interaction;
using TerrariaOverhaul.Core.Configuration;
using TerrariaOverhaul.Core.ItemComponents;
using TerrariaOverhaul.Core.ItemOverhauls;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.Melee;

public class Hammer : ItemOverhaul
{
	public static readonly SoundStyle HammerNormalSwing = new($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Items/Melee/BluntSwingHeavy", 4) {
		Volume = 0.75f,
		PitchVariance = 0.1f,
	};
	public static readonly SoundStyle HammerChargedSwing = new($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Items/Melee/BluntSwingSuperHeavy") {
		Volume = 0.5f,
		PitchVariance = 0.1f,
	};

	public static readonly ConfigEntry<bool> EnableHammerPowerAttacks = new(ConfigSide.Both, "Melee", nameof(EnableHammerPowerAttacks), () => true);

	public override bool ShouldApplyItemOverhaul(Item item)
	{
		// Must have hammering capabilities
		if (item.hammer <= 0) {
			return false;
		}

		// Avoid pickaxes, axes, and placeables
		if (item.pick > 0 || item.axe > 0 || item.createTile >= TileID.Dirt || item.createWall >= 0) {
			return false;
		}

		// Hammers always swing, deal melee damage, don't have channeling, and are visible
		if (item.useStyle != ItemUseStyleID.Swing || item.noMelee || item.channel || item.noUseGraphic) {
			return false;
		}

		return true;
	}

	public override void SetDefaults(Item item)
	{
		// Defaults

		if (item.UseSound.HasValue && !item.UseSound.Value.IsTheSameAs(SoundID.Item15)) {
			item.UseSound = HammerNormalSwing;
		}

		item.knockBack *= 1.5f; // Increased knockback

		// Components

		if (ItemMeleeAirCombat.EnableAirCombat) {
			item.EnableComponent<ItemMeleeAirCombat>();
		}

		if (ItemMeleeSwingVelocity.EnableSwingVelocity) {
			item.EnableComponent<ItemMeleeSwingVelocity>(c => {
				c.DashVelocity = new Vector2(3.0f, 3.5f);
				c.MaxDashVelocity = new Vector2(0.0f, 5.0f);

				c.AddVelocityModifier(in ItemMeleeSwingVelocity.Modifiers.PowerAttackBoost);
				c.AddVelocityModifier(in ItemMeleeSwingVelocity.Modifiers.PowerAttackGroundBoost);
				c.AddVelocityModifier(in ItemMeleeSwingVelocity.Modifiers.DisableVerticalDashesForNonChargedAttacks);
				c.AddVelocityModifier(in ItemMeleeSwingVelocity.Modifiers.DisableUpwardsDashesWhenFalling);
				c.AddVelocityModifier(in ItemMeleeSwingVelocity.Modifiers.DisableDashesForNonChargedAttacksWhenStill);
			});
		}

		item.EnableComponent<ItemMeleeGoreInteraction>();
		item.EnableComponent<ItemMeleeNpcStuns>();
		item.EnableComponent<ItemMeleeCooldownReplacement>();
		item.EnableComponent<ItemMeleeAttackAiming>();
		item.EnableComponent<ItemVelocityBasedDamage>();

		// Animation
		item.EnableComponent<QuickSlashMeleeAnimation>(c => {
			c.FlipAttackEachSwing = true;
			c.AnimateLegs = true;
		});

		// Power Attacks
		if (EnableHammerPowerAttacks) {
			item.EnableComponent<ItemMeleePowerAttackEffects>();
			item.EnableComponent<ItemPowerAttacks>(c => {
				c.ChargeLengthMultiplier = 1.5f;

				var modifiers = new CommonStatModifiers();

				modifiers.MeleeDamageMultiplier = modifiers.ProjectileDamageMultiplier = 1.5f;
				modifiers.MeleeKnockbackMultiplier = modifiers.ProjectileKnockbackMultiplier = 2.0f; // Even more knockback
				modifiers.MeleeRangeMultiplier = 1.4f;
				modifiers.ProjectileSpeedMultiplier = 1.5f;

				c.StatModifiers.Single = modifiers;
			});

			if (!Main.dedServ) {
				item.EnableComponent<ItemPowerAttackSounds>(c => {
					c.Sound = HammerChargedSwing;
					c.ReplacesUseSound = true;
				});
			}
		}
	}
}

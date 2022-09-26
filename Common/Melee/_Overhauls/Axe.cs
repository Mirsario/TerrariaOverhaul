using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using TerrariaOverhaul.Common.BloodAndGore;
using TerrariaOverhaul.Common.Charging;
using TerrariaOverhaul.Core.Configuration;
using TerrariaOverhaul.Core.ItemComponents;
using TerrariaOverhaul.Core.ItemOverhauls;

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
		// Defaults

		if (item.UseSound.HasValue && !item.UseSound.Value.IsTheSameAs(SoundID.Item15)) {
			item.UseSound = AxeNormalSwingSound;
		}

		// Components

		item.EnableComponent<ItemMeleeGoreInteraction>();
		if (ItemMeleeAirCombat.EnableAirCombat)
			item.EnableComponent<ItemMeleeAirCombat>();
		item.EnableComponent<ItemMeleeNpcStuns>();
		item.EnableComponent<ItemMeleeCooldownReplacement>();
		item.EnableComponent<ItemMeleeAttackAiming>();
		item.EnableComponent<ItemVelocityBasedDamage>();
		item.EnableComponent<ItemMeleeSwingVelocity>(c => {
			c.DashVelocity = new Vector2(2.0f, 4.5f);
			c.MaxDashVelocity = new Vector2(0f, 6.0f);

			c.AddVelocityModifier(in ItemMeleeSwingVelocity.Modifiers.PowerAttackBoost);
			c.AddVelocityModifier(in ItemMeleeSwingVelocity.Modifiers.PowerAttackVerticalGroundBoost);
			c.AddVelocityModifier(in ItemMeleeSwingVelocity.Modifiers.DisableVerticalDashesForNonChargedAttacks);
			c.AddVelocityModifier(in ItemMeleeSwingVelocity.Modifiers.DisableUpwardsDashesWhenFalling);
			c.AddVelocityModifier(in ItemMeleeSwingVelocity.Modifiers.DisableDashesForNonChargedAttacksWhenStill);
		});
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
				c.CommonStatMultipliers.MeleeRangeMultiplier = 1.4f;
				c.CommonStatMultipliers.MeleeDamageMultiplier = c.CommonStatMultipliers.ProjectileDamageMultiplier = 1.5f;
				c.CommonStatMultipliers.MeleeKnockbackMultiplier = c.CommonStatMultipliers.ProjectileKnockbackMultiplier = 1.5f;
				c.CommonStatMultipliers.ProjectileSpeedMultiplier = 1.5f;
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

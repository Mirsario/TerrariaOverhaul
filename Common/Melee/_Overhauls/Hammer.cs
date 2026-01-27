// Copyright (c) 2020-2026 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.Charging;
using TerrariaOverhaul.Common.Interaction;
using TerrariaOverhaul.Common.Items;
using TerrariaOverhaul.Core.Configuration;
using TerrariaOverhaul.Core.ItemComponents;
using TerrariaOverhaul.Core.ItemOverhauls;
using TerrariaOverhaul.Utilities.Terraria;

namespace TerrariaOverhaul.Common.Melee;

internal class Hammer : ItemOverhaul
{
	public static readonly SoundStyle HammerNormalSwing = new($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Items/Melee/BluntSwingHeavy", 4) {
		Volume = 0.75f,
		PitchVariance = 0.1f,
	};
	public static readonly SoundStyle HammerChargedSwing = new($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Items/Melee/BluntSwingSuperHeavy") {
		Volume = 0.5f,
		PitchVariance = 0.1f,
	};

	public static readonly ConfigEntry<bool> EnableHammerPowerAttacks = new(ConfigSide.Both, true, "Melee");
	public static readonly ConfigEntry<bool> EnableHammerSoundReplacements = new(ConfigSide.ClientOnly, true, "Melee");

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

		if (EnableHammerSoundReplacements && item.UseSound.HasValue && !item.UseSound.Value.IsTheSameAs(SoundID.Item15)) {
			item.UseSound = HammerNormalSwing;
		}

		item.knockBack *= 1.5f; // Increased knockback

		// Components

		if (ItemMeleeAirCombat.EnableMeleeAirCombat) {
			item.EnableComponent<ItemMeleeAirCombat>();
		}

		if (ItemMeleeSwingVelocity.EnableMeleeSwingVelocity) {
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

	public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
	{
		base.ModifyTooltips(item, tooltips);

		IEnumerable<string> GetCombatInfo()
		{
			yield return Mod.GetTextValue("ItemOverhauls.Melee.PowerStrikeInfo");
			yield return Mod.GetTextValue("ItemOverhauls.Melee.AirCombatInfo");
			yield return Mod.GetTextValue("ItemOverhauls.Melee.VelocityBasedDamageInfo");
		}

		ItemTooltips.ShowCombatInformation(Mod, tooltips, GetCombatInfo);
	}
}

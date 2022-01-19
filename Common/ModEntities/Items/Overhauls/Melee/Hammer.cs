using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.ModEntities.Items.Components;
using TerrariaOverhaul.Common.ModEntities.Items.Components.Animations;
using TerrariaOverhaul.Common.ModEntities.Items.Components.Melee;

namespace TerrariaOverhaul.Common.ModEntities.Items.Overhauls
{
	public class Hammer : ItemOverhaul
	{
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

			if (item.UseSound is LegacySoundStyle && item.UseSound != SoundID.Item15) {
				item.UseSound = new ModSoundStyle($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Items/Melee/BluntSwingHeavy", 4, volume: 0.75f, pitchVariance: 0.1f);
			}

			item.knockBack *= 1.5f; // Increased knockback

			// Components

			item.EnableComponent<ItemMeleeGoreInteraction>();
			item.EnableComponent<ItemMeleeNpcStuns>();
			item.EnableComponent<ItemMeleeCooldownDisabler>();
			item.EnableComponent<ItemMeleeAttackAiming>();
			item.EnableComponent<ItemVelocityBasedDamage>();
			// Animation
			item.EnableComponent<QuickSlashMeleeAnimation>(c => {
				c.FlipAttackEachSwing = true;
				c.AnimateLegs = true;
			});

			// Power Attacks
			item.EnableComponent<ItemMeleePowerAttackEffects>();
			item.EnableComponent<ItemPowerAttacks>(c => {
				c.ChargeLengthMultiplier = 1.5f;
				c.CommonStatMultipliers.MeleeRangeMultiplier = 1.4f;
				c.CommonStatMultipliers.MeleeDamageMultiplier = c.CommonStatMultipliers.ProjectileDamageMultiplier = 1.5f;
				c.CommonStatMultipliers.MeleeKnockbackMultiplier = c.CommonStatMultipliers.ProjectileKnockbackMultiplier = 2.0f; // Even more knockback
			});

			item.EnableComponent<ItemPowerAttackSounds>(c => {
				c.Sound = new ModSoundStyle($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Items/Melee/BluntSwingSuperHeavy", pitchVariance: 0.1f);
				c.ReplacesUseSound = true;
			});
		}
	}
}

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.BloodAndGore;
using TerrariaOverhaul.Common.Camera;
using TerrariaOverhaul.Common.Charging;
using TerrariaOverhaul.Common.Hooks.Items;
using TerrariaOverhaul.Core.Configuration;
using TerrariaOverhaul.Core.ItemComponents;
using TerrariaOverhaul.Core.ItemOverhauls;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.Melee;

public partial class Broadsword : ItemOverhaul, ICanDoMeleeDamage, IModifyItemNPCHitSound
{
	public static readonly SoundStyle SwordMediumSwing = new($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Items/Melee/CuttingSwingMedium", 2) {
		Volume = 0.8f,
		PitchVariance = 0.1f,
	};
	public static readonly SoundStyle SwordHeavySwing = new($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Items/Melee/CuttingSwingHeavy", 2) {
		PitchVariance = 0.1f,
	};
	public static readonly SoundStyle SwordFleshHitSound = new($"{nameof(TerrariaOverhaul)}/Assets/Sounds/HitEffects/SwordFleshHit", 2) {
		Volume = 0.65f,
		PitchVariance = 0.1f
	};

	public static readonly ConfigEntry<bool> EnableBroadswordPowerAttacks = new(ConfigSide.Both, "Melee", nameof(EnableBroadswordPowerAttacks), () => true);

	public override bool ShouldApplyItemOverhaul(Item item)
	{
		// Broadswords always swing, deal melee damage, don't have channeling, and are visible
		if (item.useStyle != ItemUseStyleID.Swing || item.noMelee || item.channel || item.noUseGraphic) {
			return false;
		}

		// Avoid tools and blocks
		if (item.pick > 0 || item.axe > 0 || item.hammer > 0 || item.createTile >= TileID.Dirt || item.createWall >= 0) {
			return false;
		}

		if (item.DamageType != DamageClass.Melee) {
			return false;
		}

		return true;
	}

	public override void SetDefaults(Item item)
	{
		base.SetDefaults(item);

		// Defaults

		if (item.UseSound.HasValue && !item.UseSound.Value.IsTheSameAs(SoundID.Item15)) {
			item.UseSound = SwordMediumSwing;
		}

		// Components

		item.EnableComponent<ItemMeleeGoreInteraction>();
		item.EnableComponent<ItemMeleeAirCombat>();
		item.EnableComponent<ItemMeleeNpcStuns>();
		item.EnableComponent<ItemMeleeCooldownDisabler>();
		item.EnableComponent<ItemMeleeAttackAiming>();
		item.EnableComponent<ItemVelocityBasedDamage>();
		item.EnableComponent<ItemMeleeSwingVelocity>(c => {
			c.DashVelocity = new Vector2(2.5f, 4.0f);
			c.MaxDashVelocity = new Vector2(0f, 5.5f);

			c.AddVelocityModifier(in ItemMeleeSwingVelocity.Modifiers.PowerAttackBoost);
			c.AddVelocityModifier(in ItemMeleeSwingVelocity.Modifiers.PowerAttackVerticalGroundBoost);
			c.AddVelocityModifier(in ItemMeleeSwingVelocity.Modifiers.DisableVerticalDashesForNonChargedAttacks);
			c.AddVelocityModifier(in ItemMeleeSwingVelocity.Modifiers.DisableUpwardsDashesWhenFalling);
		});
		// Animation
		item.EnableComponent<QuickSlashMeleeAnimation>(c => {
			c.FlipAttackEachSwing = true;
			c.AnimateLegs = true;
		});

		// Power Attacks
		if (EnableBroadswordPowerAttacks) {
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
					c.Sound = SwordHeavySwing;
					c.ReplacesUseSound = true;
				});
			}
		}

		// Killing Blows
		item.EnableComponent<ItemKillingBlows>();
	}

	public override void UseAnimation(Item item, Player player)
	{
		// Slight screenshake for the swing.
		if (!Main.dedServ) {
			ScreenShakeSystem.New(3f, item.useAnimation / 120f);
		}
	}

	public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
	{
		base.ModifyTooltips(item, tooltips);

		IEnumerable<string> GetCombatInfo()
		{
			yield return Mod.GetTextValue("ItemOverhauls.Melee.Broadsword.KillingBlowInfo");
			yield return Mod.GetTextValue("ItemOverhauls.Melee.AirCombatInfo");
			yield return Mod.GetTextValue("ItemOverhauls.Melee.VelocityBasedDamageInfo");
		}

		TooltipUtils.ShowCombatInformation(Mod, tooltips, GetCombatInfo);
	}

	public bool CanDoMeleeDamage(Item item, Player player)
	{
		// Damage will only be applied during the first half of the use. The second half is a cooldown, and the animations reflect that.
		return player.itemAnimation >= player.itemAnimationMax / 2 && !item.GetGlobalItem<ItemCharging>().IsCharging;
	}

	void IModifyItemNPCHitSound.ModifyItemNPCHitSound(Item item, Player player, NPC target, ref SoundStyle? customHitSound, ref bool playNPCHitSound)
	{
		// This checks for whether or not the target has bled.
		if (target.TryGetGlobalNPC(out NPCBloodAndGore npcBloodAndGore) && npcBloodAndGore.LastHitBloodAmount > 0) {
			customHitSound = SwordFleshHitSound;
		}
	}
}

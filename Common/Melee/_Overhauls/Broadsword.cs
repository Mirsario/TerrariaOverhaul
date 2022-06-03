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
using TerrariaOverhaul.Core.ItemComponents;
using TerrariaOverhaul.Core.ItemOverhauls;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.Melee
{
	public partial class Broadsword : ItemOverhaul, ICanDoMeleeDamage, IModifyItemNPCHitSound
	{
		public static readonly SoundStyle SwordMediumSwing = new($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Items/Melee/CuttingSwingMedium", 2) {
			PitchVariance = 0.1f,	
		};
		public static readonly SoundStyle SwordHeavySwing = new($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Items/Melee/CuttingSwingHeavy", 2) {
			PitchVariance = 0.1f,
		};
		public static readonly SoundStyle SwordFleshHitSound = new($"{nameof(TerrariaOverhaul)}/Assets/Sounds/HitEffects/SwordFleshHit", 2) {
			Volume = 0.65f,
			PitchVariance = 0.1f
		};

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
				c.CommonStatMultipliers.MeleeKnockbackMultiplier = c.CommonStatMultipliers.ProjectileKnockbackMultiplier = 1.5f;
				c.CommonStatMultipliers.ProjectileSpeedMultiplier = 1.5f;
			});

			if (!Main.dedServ) {
				item.EnableComponent<ItemPowerAttackSounds>(c => {
					c.Sound = SwordHeavySwing;
					c.ReplacesUseSound = true;
				});
			}

			// Killing Blows
			item.EnableComponent<ItemKillingBlows>();
		}

		public override void UseAnimation(Item item, Player player)
		{
			var powerAttacks = item.GetGlobalItem<ItemPowerAttacks>();

			// Swing velocity

			// TML Problem:
			// Couldn't just use MeleeAttackAiming.AttackDirection here due to TML lacking proper tools for controlling execution orders.
			// By chance, this global's hooks run before MeleeAttackAiming's.
			// -- Mirsario
			var attackDirection = player.LookDirection();

			int totalAnimationTime = CombinedHooks.TotalAnimationTime(item.useAnimation, player, item);
			var dashSpeed = new Vector2(
				totalAnimationTime / 7f,
				totalAnimationTime / 13f
			);

			if (powerAttacks.PowerAttack) {
				dashSpeed.X *= 1.5f;
				dashSpeed.Y *= 2.2f;

				if (player.OnGround()) {
					dashSpeed.Y *= 1.65f;
				}
			} else {
				if (player.OnGround()) {
					// Disable vertical dashes for non-charged attacks whenever the player is on ground.
					// Also reduces horizontal movement.
					dashSpeed.X *= 0.625f;
					dashSpeed.Y = 0f;
				} else if (attackDirection.Y < 0f && player.velocity.Y > 0f) {
					// Disable upwards dashes whenever the player is falling down.
					dashSpeed.Y = 0f;
				}

				// Disable horizontal dashes whenever the player is holding a directional key opposite to the direction of the dash.
				if (player.KeyDirection() == -Math.Sign(attackDirection.X)) {
					dashSpeed.X = 0f;
				}
			}

			player.AddLimitedVelocity(dashSpeed * attackDirection, new Vector2(dashSpeed.X, 12f));

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
}

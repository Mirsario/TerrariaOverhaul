using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.ItemAnimations;
using TerrariaOverhaul.Common.ModEntities.Items.Shared;
using TerrariaOverhaul.Common.ModEntities.Items.Shared.Melee;
using TerrariaOverhaul.Common.ModEntities.NPCs;
using TerrariaOverhaul.Common.Systems.Camera.ScreenShakes;
using TerrariaOverhaul.Common.Systems.Time;
using TerrariaOverhaul.Utilities;
using TerrariaOverhaul.Utilities.DataStructures;
using TerrariaOverhaul.Utilities.Enums;
using TerrariaOverhaul.Utilities.Extensions;

namespace TerrariaOverhaul.Common.ModEntities.Items.Overhauls
{
	public partial class Broadsword : MeleeWeapon
	{
		public static readonly ModSoundStyle SwordFleshHitSound = new($"{nameof(TerrariaOverhaul)}/Assets/Sounds/HitEffects/SwordFleshHit", 2, volume: 0.65f, pitchVariance: 0.1f);

		private PowerAttacks powerAttacks;
		private KillingBlows killingBlows;

		public override MeleeAnimation Animation => ModContent.GetInstance<QuickSlashMeleeAnimation>();

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

		public override void SetDefaults(Item item)
		{
			base.SetDefaults(item);

			// Power Attacks

			powerAttacks = item.GetGlobalItem<PowerAttacks>();
			powerAttacks.Enabled = true;
			powerAttacks.ChargeLengthMultiplier = 1.5f;
			powerAttacks.CommonStatMultipliers.MeleeRangeMultiplier = 1.4f;
			powerAttacks.CommonStatMultipliers.MeleeDamageMultiplier = powerAttacks.CommonStatMultipliers.ProjectileDamageMultiplier = 1.5f;
			powerAttacks.CommonStatMultipliers.MeleeKnockbackMultiplier = powerAttacks.CommonStatMultipliers.ProjectileKnockbackMultiplier = 1.5f;

			powerAttacks.OnChargeStart += (item, player, chargeLength) => {
				//These 2 lines only affect animations.
				FlippedAttack = false;
				AttackDirection = Vector2.UnitX * player.direction;
			};
			powerAttacks.OnChargeUpdate += (item, player, chargeLength, progress) => {
				var broadsword = item.GetGlobalItem<Broadsword>();

				// Only visual
				broadsword.AttackDirection = Vector2.Lerp(broadsword.AttackDirection, player.LookDirection(), 5f * TimeSystem.LogicDeltaTime);
			};

			// Killing Blows

			killingBlows = item.GetGlobalItem<KillingBlows>();
			killingBlows.Enabled = true;
		}
		
		public override void UseAnimation(Item item, Player player)
		{
			base.UseAnimation(item, player);

			if(!powerAttacks.PowerAttack) {
				FlippedAttack = AttackNumber % 2 != 0;
			}

			//Swing velocity

			int totalAnimationTime = CombinedHooks.TotalAnimationTime(item.useAnimation, player, item);
			Vector2 dashSpeed = new Vector2(
				totalAnimationTime / 7f,
				totalAnimationTime / 13f
			);
			 
			if(powerAttacks.PowerAttack) {
				dashSpeed.X *= 1.5f;
				dashSpeed.Y *= 2.2f;

				if(player.OnGround()) {
					dashSpeed.Y *= 1.65f;
				}
			} else {
				if(player.OnGround()) {
					//Disable vertical dashes for non-charged attacks whenever the player is on ground.
					//Also reduces horizontal movement.
					dashSpeed.X *= 0.625f;
					dashSpeed.Y = 0f;
				} else if(AttackDirection.Y < 0f && player.velocity.Y > 0f) {
					//Disable upwards dashes whenever the player is falling down.
					dashSpeed.Y = 0f;
				}

				//Disable horizontal dashes whenever the player is holding a directional key opposite to the direction of the dash.
				if(player.KeyDirection() == -Math.Sign(AttackDirection.X)) {
					dashSpeed.X = 0f;
				}
			}

			player.AddLimitedVelocity(dashSpeed * AttackDirection, new Vector2(dashSpeed.X, 12f));

			//Slight screenshake for the swing.
			if(!Main.dedServ) {
				ScreenShakeSystem.New(3f, item.useAnimation / 120f);
			}
		}
		
		public override void UseItemFrame(Item item, Player player)
		{
			base.UseItemFrame(item, player);

			//Leg frame
			if(player.velocity.Y == 0f && player.KeyDirection() == 0) {
				if(Math.Abs(AttackDirection.X) > 0.5f) {
					player.legFrame = (FlippedAttack ? PlayerFrames.Walk8 : PlayerFrames.Jump).ToRectangle();
				} else {
					player.legFrame = PlayerFrames.Walk13.ToRectangle();
				}
			}
		}
		
		public override bool ShouldBeAttacking(Item item, Player player)
		{
			//Damage will only be applied during the first half of the use. The second half is a cooldown, and the animations reflect that.
			return base.ShouldBeAttacking(item, player) && player.itemAnimation >= player.itemAnimationMax / 2 && !item.GetGlobalItem<ItemCharging>().IsCharging;
		}

		public override void ModifyItemNPCHitSound(Item item, Player player, NPC target, ref SoundStyle customHitSound, ref bool playNPCHitSound)
		{
			//This checks for whether or not the target has bled.
			if(target.TryGetGlobalNPC(out NPCBloodAndGore npcBloodAndGore) && npcBloodAndGore.LastHitBloodAmount > 0) {
				customHitSound = SwordFleshHitSound;
			}

			base.ModifyItemNPCHitSound(item, player, target, ref customHitSound, ref playNPCHitSound);
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
	}
}

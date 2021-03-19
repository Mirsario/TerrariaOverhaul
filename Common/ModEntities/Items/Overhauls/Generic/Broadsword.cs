using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.ItemAnimations;
using TerrariaOverhaul.Common.ModEntities.Items.Utilities;
using TerrariaOverhaul.Common.ModEntities.NPCs;
using TerrariaOverhaul.Common.Systems.Camera.ScreenShakes;
using TerrariaOverhaul.Utilities.Enums;
using TerrariaOverhaul.Utilities.Extensions;

namespace TerrariaOverhaul.Common.ModEntities.Items.Overhauls.Generic
{
	public partial class Broadsword : MeleeWeapon
	{
		public static readonly ModSoundStyle SwordFleshHitSound = new ModSoundStyle($"{nameof(TerrariaOverhaul)}/Assets/Sounds/HitEffects/SwordFleshHit", 2, volume: 0.65f, pitchVariance: 0.1f);

		public override MeleeAnimation Animation => ModContent.GetInstance<QuickSlashMeleeAnimation>();

		public override void Load()
		{
			base.Load();

			LoadKillingBlows();
		}

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
		public override void HoldItem(Item item, Player player)
		{
			HoldItemCharging(item, player);
			
			base.HoldItem(item, player);
		}
		public override void UseAnimation(Item item, Player player)
		{
			base.UseAnimation(item, player);

			if(!ChargedAttack) {
				FlippedAttack = AttackNumber % 2 != 0;
			}

			//Swing velocity

			Vector2 dashSpeed = Vector2.One * (PlayerHooks.TotalMeleeTime(item.useAnimation, player, item) / 7f);

			if(ChargedAttack) {
				dashSpeed *= 1.5f;

				if(player.OnGround()) {
					dashSpeed.Y *= 1.5f;
				}
			} else {
				//Disable vertical dashes for non-charged attacks whenever the player is on ground.
				if(player.OnGround()) {
					dashSpeed.Y = 0f;
				}

				//Disable horizontal dashes whenever the player is holding a directional key opposite to the direction of the dash.
				if(player.KeyDirection() == -Math.Sign(AttackDirection.X)) {
					dashSpeed.X = 0f;
				}
			}

			BasicVelocityDash(player, dashSpeed * AttackDirection, new Vector2(dashSpeed.X, float.PositiveInfinity));

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
		public override void ModifyHitNPC(Item item, Player player, NPC target, ref int damage, ref float knockback, ref bool crit)
		{
			base.ModifyHitNPC(item, player, target, ref damage, ref knockback, ref crit);

			ModifyHitNPCCharging(item, player, target, ref damage, ref knockback, ref crit);
			ModifyHitNPCKillingBlows(item, player, target, ref damage, ref knockback, ref crit);
		}
		public override void ModifyItemNPCHitSound(Item item, Player player, NPC target, ref SoundStyle customHitSound, ref bool playNPCHitSound)
		{
			//This checks for whether or not the target has bled.
			if(target.TryGetGlobalNPC(out NPCBloodAndGore npcBloodAndGore) && npcBloodAndGore.LastHitBloodAmount > 0) {
				customHitSound = SwordFleshHitSound;
			}

			base.ModifyItemNPCHitSound(item, player, target, ref customHitSound, ref playNPCHitSound);
		}
	}
}

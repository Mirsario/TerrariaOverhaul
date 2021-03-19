using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.ItemAnimations;
using TerrariaOverhaul.Common.ModEntities.Items.Utilities;
using TerrariaOverhaul.Common.ModEntities.NPCs;
using TerrariaOverhaul.Common.ModEntities.Players;
using TerrariaOverhaul.Common.Systems.Camera.ScreenShakes;
using TerrariaOverhaul.Common.Systems.Time;
using TerrariaOverhaul.Utilities.DataStructures;
using TerrariaOverhaul.Utilities.Enums;
using TerrariaOverhaul.Utilities.Extensions;

namespace TerrariaOverhaul.Common.ModEntities.Items.Overhauls.Generic
{
	public class Broadsword : MeleeWeapon
	{
		public static readonly ModSoundStyle SwordFleshHitSound = new ModSoundStyle($"{nameof(TerrariaOverhaul)}/Assets/Sounds/HitEffects/SwordFleshHit", 2, volume: 0.65f, pitchVariance: 0.1f);

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

			//item.useAnimation /= 2;
			//item.useTime /= 2;
			//item.reuseDelay += item.useAnimation;
		}
		public override void UseAnimation(Item item, Player player)
		{
			base.UseAnimation(item, player);

			FlippedAttack = AttackNumber % 2 != 0;

			Vector2 dashSpeed = Vector2.One * (PlayerHooks.TotalMeleeTime(item.useAnimation, player, item) / 7f);

			//Disable vertical dashes for non-charged attacks whenever the player is on ground.
			if(player.OnGround()) {
				dashSpeed.Y = 0f;
			}

			//Disable horizontal dashes whenever the player is holding a directional key opposite to the direction of the dash.
			if(player.KeyDirection() == -Math.Sign(AttackDirection.X)) {
				dashSpeed.X = 0f;
			}

			BasicVelocityDash(player, dashSpeed * AttackDirection, new Vector2(dashSpeed.X, float.PositiveInfinity));

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
			return base.ShouldBeAttacking(item, player) && player.itemAnimation >= player.itemAnimationMax / 2;
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

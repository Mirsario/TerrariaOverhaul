using System;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.Hooks.Items;
using TerrariaOverhaul.Common.ItemAnimations;
using TerrariaOverhaul.Common.ModEntities.Items.Components;
using TerrariaOverhaul.Common.ModEntities.Items.Components.Melee;
using TerrariaOverhaul.Common.ModEntities.NPCs;
using TerrariaOverhaul.Common.Tags;
using TerrariaOverhaul.Core.Exceptions;
using TerrariaOverhaul.Utilities;
using TerrariaOverhaul.Utilities.Extensions;

namespace TerrariaOverhaul.Common.ModEntities.Items.Overhauls
{
	public class MeleeWeapon : ItemOverhaul, IModifyItemNPCHitSound
	{
		public static readonly ModSoundStyle WoodenHitSound = new($"{nameof(TerrariaOverhaul)}/Assets/Sounds/HitEffects/WoodenHit", 3, volume: 0.3f, pitchVariance: 0.1f);

		protected ItemMeleeAttackAiming MeleeAttackAiming { get; private set; }
		protected ItemPlayerAnimator ItemPlayerAnimator { get; private set; }

		public virtual float GetHeavyness(Item item)
		{
			const float HeaviestSpeed = 0.5f;
			const float LightestSpeed = 5f;

			float speed = 1f / (Math.Max(1, item.useAnimation) / 60f);
			float speedResult = MathHelper.Clamp(MathUtils.InverseLerp(speed, LightestSpeed, HeaviestSpeed), 0f, 1f);

			float averageDimension = (item.width + item.height) * 0.5f;
			float sizeResult = Math.Max(0f, (averageDimension) / 10f);

			float result = speedResult * sizeResult;

			return MathHelper.Clamp(result, 0f, 1f);
		}

		public override bool ShouldApplyItemOverhaul(Item item) => false;

		public override void Load()
		{
			base.Load();

			if (GetType() == typeof(MeleeWeapon)) {
				// Disable attackCD for melee.
				IL.Terraria.Player.ItemCheck_MeleeHitNPCs += context => {
					var cursor = new ILCursor(context);

					if (!cursor.TryGotoNext(
						MoveType.Before,
						i => i.Match(OpCodes.Ldarg_0),
						i => i.Match(OpCodes.Ldc_I4_1),
						i => i.Match(OpCodes.Ldarg_0),
						i => i.MatchLdfld(typeof(Player), nameof(Player.itemAnimationMax)),
						i => i.Match(OpCodes.Conv_R8),
						i => i.MatchLdcR8(0.33d),
						i => i.Match(OpCodes.Mul),
						i => i.Match(OpCodes.Conv_I4),
						i => i.MatchCall(typeof(Math), nameof(Math.Max)),
						i => i.MatchStfld(typeof(Player), nameof(Player.attackCD))
					)) {
						throw new ILMatchException(context, "Disabling attackCD: Match 1", this);
					}

					//TODO: Instead of removing the code, skip over it if the item has a MeleeWeapon overhaul
					cursor.RemoveRange(10);
				};
			}
		}

		public override GlobalItem Clone(Item item, Item itemClone)
		{
			var clone = (MeleeWeapon)base.Clone(item, itemClone);

			clone.MeleeAttackAiming = itemClone.GetGlobalItem<ItemMeleeAttackAiming>();
			clone.ItemPlayerAnimator = itemClone.GetGlobalItem<ItemPlayerAnimator>();

			return clone;
		}

		public override void SetDefaults(Item item)
		{
			base.SetDefaults(item);

			if (item.UseSound != Terraria.ID.SoundID.Item15) {
				float heavyness = GetHeavyness(item);
				float averageDimension = (item.width + item.height) * 0.5f;

				item.UseSound = averageDimension switch {
					//>= 38 => new ModSoundStyle($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Items/Melee/CuttingSwingHeavy", 2),
					>= 25 => new ModSoundStyle($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Items/Melee/CuttingSwingMedium", 3),
					_ => new ModSoundStyle($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Items/Melee/CuttingSwingLightAlt", 3)
				};
			}

			MeleeAttackAiming = item.EnableComponent<ItemMeleeAttackAiming>();

			ItemPlayerAnimator = item.EnableComponent<ItemPlayerAnimator>(c => {
				c.Animation = ModContent.GetInstance<GenericMeleeAnimation>();
			});
		}

		// Hitting

		public override void ModifyHitNPC(Item item, Player player, NPC target, ref int damage, ref float knockback, ref bool crit)
		{
			base.ModifyHitNPC(item, player, target, ref damage, ref knockback, ref crit);

			// Make directional knockback work with melee.
			if (target.TryGetGlobalNPC(out NPCDirectionalKnockback npcKnockback)) {
				npcKnockback.SetNextKnockbackDirection(MeleeAttackAiming.AttackDirection);
			}

			// Reduce knockback when the player is in air, and the enemy is somewhat above them.
			if (!player.OnGround() && MeleeAttackAiming.AttackDirection.Y < 0.25f) {
				knockback *= 0.75f;
			}
		}

		public override void OnHitNPC(Item item, Player player, NPC target, int damage, float knockback, bool crit)
		{
			base.OnHitNPC(item, player, target, damage, knockback, crit);

			target.GetGlobalNPC<NPCAttackCooldowns>().SetAttackCooldown(target, player.itemAnimationMax, true);

			/*if(player.velocity.Y != 0f || Math.Abs(target.oldVelocity.Y) > 0.3f) {
				target.GetGlobalNPC<NPCFreezeFrames>().SetFreezeFrames(target, Math.Min(5, player.itemAnimationMax / 3));
			}*/

			var movement = player.GetModPlayer<Players.PlayerMovement>();
			var modifier = Players.PlayerMovement.MovementModifier.Default;

			if (player.velocity.Y != 0f) {
				if (MeleeAttackAiming.AttackDirection.Y < 0.1f) {
					modifier.gravityScale *= 0.1f;
				}

				var positionDifference = target.Center - player.Center;
				float distance = positionDifference.SafeLength();
				var dashDirection = target.velocity.SafeNormalize(default);
				var dashVelocity = dashDirection;

				// Boost velocity is based on item knockback.
				float targetSpeed = target.velocity.SafeLength();

				dashVelocity *= Math.Min(Math.Max(2f, targetSpeed), distance / 3f);

				// Reduce intensity when the player is not directly aiming at the enemy.
				float directionsDotProduct = Vector2.Dot(dashDirection, MeleeAttackAiming.AttackDirection);

				dashVelocity *= Math.Max(0f, directionsDotProduct * directionsDotProduct);

				// Slight upwards movement bonus.
				dashVelocity.Y -= 1.5f;

				var maxVelocity = Vector2.Min(Vector2.One * 9f, new Vector2(Math.Abs(dashVelocity.X), Math.Abs(dashVelocity.Y)));

				player.AddLimitedVelocity(dashVelocity, maxVelocity);
			}

			movement.SetMovementModifier($"{nameof(MeleeWeapon)}/{nameof(OnHitNPC)}", player.itemAnimationMax / 2, modifier);
		}

		public virtual void ModifyItemNPCHitSound(Item item, Player player, NPC target, ref ISoundStyle customHitSound, ref bool playNPCHitSound)
		{
			if (OverhaulItemTags.Wooden.Has(item.netID)) {
				customHitSound = WoodenHitSound;
			}
		}
	}
}

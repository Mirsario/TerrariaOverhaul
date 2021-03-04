using System;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.ItemAnimations;
using TerrariaOverhaul.Common.ModEntities.NPCs;
using TerrariaOverhaul.Common.SoundStyles;
using TerrariaOverhaul.Common.Systems.CombatTexts;
using TerrariaOverhaul.Common.Systems.Gores;
using TerrariaOverhaul.Core.Systems.Debugging;
using TerrariaOverhaul.Utilities;
using TerrariaOverhaul.Utilities.DataStructures;
using TerrariaOverhaul.Utilities.Enums;
using TerrariaOverhaul.Utilities.Extensions;

namespace TerrariaOverhaul.Common.ModEntities.Items.Overhauls.Generic
{
	public class MeleeWeapon : ItemOverhaul
	{
		private static readonly Gradient<Color> DamageScaleColor = new Gradient<Color>(
			(0f, Color.Black),
			(1f, Color.LightGray),
			(1.25f, Color.Green),
			(1.75f, Color.Yellow),
			(2.5f, Color.Red)
		);

		public bool FlippedAttack { get; protected set; }
		public Vector2 AttackDirection { get; private set; }
		public float AttackAngle { get; private set; }
		public int AttackNumber { get; private set; }

		public virtual bool VelocityBasedDamage => true;
		public virtual MeleeAnimation Animation => ModContent.GetInstance<GenericMeleeAnimation>();

		public virtual float GetAttackRange(Item item)
		{
			return (item.Size * item.scale * 1.25f).Length();
		}
		public virtual float GetHeavyness(Item item)
		{
			float averageDimension = (item.width + item.height) * 0.5f;

			const float HeaviestSpeed = 0.5f;
			const float LightestSpeed = 5f;

			float speed = 1f / (Math.Max(1, item.useAnimation) / 60f);
			float speedResult = MathHelper.Clamp(MathUtils.InverseLerp(speed, LightestSpeed, HeaviestSpeed), 0f, 1f);
			float sizeResult = Math.Max(0f, (averageDimension) / 10f);

			float result = speedResult;

			return MathHelper.Clamp(result, 0f, 1f);
		}
		public virtual bool ShouldBeAttacking(Item item, Player player)
		{
			return player.itemAnimation > 0;
		}

		public override bool ShouldApplyItemOverhaul(Item item) => false;
		public override void Load()
		{
			base.Load();

			if(GetType() == typeof(MeleeWeapon)) {
				//Disable attackCD for melee.
				IL.Terraria.Player.ItemCheck_MeleeHitNPCs += context => {
					var cursor = new ILCursor(context);

					if(!cursor.TryGotoNext(
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
						throw new Exception($"{nameof(Broadsword)}: IL Failure.");
					}

					//TODO: Instead of removing the code, skip over it if the item has a MeleeWeapon overhaul
					cursor.RemoveRange(10);
				};
			}
		}
		public override void SetDefaults(Item item)
		{
			if(item.UseSound != Terraria.ID.SoundID.Item15) {
				item.UseSound = new BlendedSoundStyle(
					new ModSoundStyle(nameof(TerrariaOverhaul), "Assets/Sounds/Items/Melee/SwingLight", 4),
					new ModSoundStyle(nameof(TerrariaOverhaul), "Assets/Sounds/Items/Melee/SwingHeavy", 4),
					GetHeavyness(item),
					0.3f
				);
			}
		}
		public override void UseAnimation(Item item, Player player)
		{
			AttackDirection = (Main.MouseWorld - player.Center).SafeNormalize(Vector2.UnitX);
			AttackAngle = AttackDirection.ToRotation();
			AttackNumber++;
		}
		public override bool? CanHitNPC(Item item, Player player, NPC target)
		{
			if(!ShouldBeAttacking(item, player)) {
				return false;
			}

			float range = GetAttackRange(item);

			return CollisionUtils.CheckRectangleVsArcCollision(target.getRect(), player.Center, AttackAngle, MathHelper.Pi * 0.5f, range);
		}
		public override void HoldItem(Item item, Player player)
		{
			base.HoldItem(item, player);

			//Hit gore.
			if(player.itemAnimation >= player.itemAnimationMax - 1 && ShouldBeAttacking(item, player)) {
				float range = GetAttackRange(item);
				float arcRadius = MathHelper.Pi * 0.5f;

				const int MaxHits = 5;

				int numHit = 0;

				for(int i = 0; i < Main.maxGore; i++) {
					if(!(Main.gore[i] is OverhaulGore gore) || !gore.active || gore.time < 30) {
						continue;
					}

					if(CollisionUtils.CheckRectangleVsArcCollision(gore.AABBRectangle, player.Center, AttackAngle, arcRadius, range)) {
						gore.HitGore(AttackDirection);

						if(++numHit >= MaxHits) {
							break;
						}
					}
				}
			}
		}
		public override void UseItemFrame(Item item, Player player)
		{
			base.UseItemFrame(item, player);

			float weaponRotation = MathUtils.Modulo(Animation.GetItemRotation(item, player), MathHelper.TwoPi);
			float pitch = MathUtils.RadiansToPitch(weaponRotation);
			var weaponDirection = weaponRotation.ToRotationVector2();

			if(Math.Sign(weaponDirection.X) != player.direction) {
				pitch = weaponDirection.Y < 0f ? 1f : 0f;
			}

			player.bodyFrame = PlayerFrames.Use3.ToRectangle();

			//Main.NewText($"{degrees:0.00} -> {i}");

			Vector2 locationOffset;

			if(pitch > 0.95f) {
				player.bodyFrame = PlayerFrames.Use1.ToRectangle();
				locationOffset = new Vector2(-8f, -9f);
			} else if(pitch > 0.7f) {
				player.bodyFrame = PlayerFrames.Use2.ToRectangle();
				locationOffset = new Vector2(4f, -8f);
			} else if(pitch > 0.3f) {
				player.bodyFrame = PlayerFrames.Use3.ToRectangle();
				locationOffset = new Vector2(4f, 2f);
			} else if(pitch > 0.05f) {
				player.bodyFrame = PlayerFrames.Use4.ToRectangle();
				locationOffset = new Vector2(4f, 7f);
			} else {
				//player.bodyFrame = PlayerFrames.Walk4.ToRectangle();
				//locationOffset = new Vector2(-8f, 2f);
				player.bodyFrame = PlayerFrames.Walk5.ToRectangle();
				locationOffset = new Vector2(-8f, 2f);
				//player.bodyFrame = PlayerFrames.Use4.ToRectangle();
				//locationOffset = new Vector2(-8f, 2f);
			}

			player.itemRotation = weaponRotation + MathHelper.PiOver4;

			if(player.direction < 0) {
				player.itemRotation += MathHelper.PiOver2;
			}

			player.itemLocation = player.Center + new Vector2(locationOffset.X * player.direction, locationOffset.Y);

			if(player.velocity.Y == 0f && player.KeyDirection() == 0) {
				if(Math.Abs(AttackDirection.X) > 0.5f) {
					player.legFrame = (FlippedAttack ? PlayerFrames.Walk8 : PlayerFrames.Jump).ToRectangle();
				} else {
					player.legFrame = PlayerFrames.Walk13.ToRectangle();
				}
			}

			if(DebugSystem.EnableDebugRendering) {
				DebugSystem.DrawCircle(player.itemLocation, 3f, Color.White);
			}
		}
		//Hitting
		public override void ModifyHitNPC(Item item, Player player, NPC target, ref int damage, ref float knockback, ref bool crit)
		{
			base.ModifyHitNPC(item, player, target, ref damage, ref knockback, ref crit);

			//Make directional knockback work with melee.
			if(target.TryGetGlobalNPC(out NPCDirectionalKnockback npcKnockback)) {
				npcKnockback.SetNextKnockbackDirection(AttackDirection);
			}

			if(VelocityBasedDamage) {
				float velocityDamageScale = Math.Max(1f, 0.78f + player.velocity.Length() / 8f);

				knockback *= velocityDamageScale;
				damage = (int)Math.Round(damage * velocityDamageScale);

				if(!Main.dedServ) {
					bool critBackup = crit;

					CombatTextSystem.AddFilter(1, text => {
						bool isCharged = false;
						string additionalInfo = $"({(critBackup ? "CRITx" : null)}{(isCharged ? "POWERx" : critBackup ? null : "x")}{velocityDamageScale:0.00})";
						float gradientScale = velocityDamageScale;

						if(critBackup) {
							gradientScale *= 2;
						}

						if(isCharged) {
							gradientScale *= 1.3f;
						}

						var font = FontAssets.CombatText[critBackup ? 1 : 0].Value;
						var size = font.MeasureString(text.text);

						text.color = DamageScaleColor.GetValue(gradientScale);
						text.position.Y -= 16f;

						/*if(headshot) {
							text.text += "!";
						}*/

						//text.text += $"\r\n{additionalInfo}";

						CombatText.NewText(new Rectangle((int)(text.position.X + size.X * 0.5f), (int)(text.position.Y + size.Y + 4), 1, 1), text.color, additionalInfo, critBackup);
					});
				}
			}
		}
		public override void OnHitNPC(Item item, Player player, NPC target, int damage, float knockBack, bool crit)
		{
			base.OnHitNPC(item, player, target, damage, knockBack, crit);

			target.GetGlobalNPC<NPCAttackCooldowns>().SetAttackCooldown(target, player.itemAnimationMax, true);

			if(player.velocity.Y != 0f || Math.Abs(target.oldVelocity.Y) > 0.3f) {
				target.GetGlobalNPC<NPCFreezeFrames>().SetFreezeFrames(target, Math.Min(5, player.itemAnimationMax / 3));
			}

			var movement = player.GetModPlayer<Players.PlayerMovement>();
			var modifier = Players.PlayerMovement.MovementModifier.Default;

			if(player.velocity.Y != 0f) {
				if(AttackDirection.Y < 0.1f) {
					modifier.gravityScale *= 0.1f;
				}

				if(AttackDirection.Y < -0.33f) {
					player.velocity.Y = Math.Min(0f, player.velocity.Y);
				}
			}

			movement.SetMovementModifier($"{nameof(MeleeWeapon)}/{nameof(OnHitNPC)}", player.itemAnimationMax / 2, modifier);
		}

		protected void BasicVelocityDash(Player player, Vector2 direction, Vector2 maxVelocity, bool powerAttack)
		{
			if(player.pulley) { // || oPlayer.OnIce) {
				return;
			}

			bool isPlayerOnGround = player.OnGround();
			float xSpeed = direction.X * maxVelocity.X;

			if(player.KeyDirection() == 0) {
				player.velocity.X = direction.X < 0f ? Math.Min(player.velocity.X, xSpeed) : Math.Max(player.velocity.X, xSpeed);
			}

			if(player.velocity.Y != 0f || powerAttack) {
				player.velocity.Y += direction.Y * maxVelocity.Y * (isPlayerOnGround ? 1f : 0.35f);
			}
		}
	}
}

using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.SoundStyles;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.ModEntities.Items.Overhauls.Generic
{
	public abstract class MeleeWeapon : ItemOverhaul
	{
		private static readonly Gradient<Color> DamageScaleColor = new Gradient<Color>(
			(0f, Color.Black),
			(1f, Color.LightGray),
			(1.25f, Color.Green),
			(1.75f, Color.Yellow),
			(2.5f, Color.Red)
		);

		public Vector2 AttackDirection { get; private set; }
		public float AttackAngle { get; private set; }

		public virtual bool VelocityBasedDamage => true;

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
		}
		public override bool? CanHitNPC(Item item, Player player, NPC target)
		{
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
		//Hitting
		public override void ModifyHitNPC(Item item, Player player, NPC target, ref int damage, ref float knockback, ref bool crit)
		{
			base.ModifyHitNPC(item, player, target, ref damage, ref knockback, ref crit);

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

			target.GetGlobalNPC<NPCAttackCooldowns>().SetAttackCooldown(target, 20, true);
		}
	}
}

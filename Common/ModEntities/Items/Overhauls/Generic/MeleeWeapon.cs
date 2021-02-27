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
		public Vector2 AttackDirection { get; private set; }
		public float AttackAngle { get; private set; }

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

		public override void SetDefaults(Item item)
		{
			item.UseSound = new BlendedSoundStyle(
				new ModSoundStyle(nameof(TerrariaOverhaul), "Assets/Sounds/Items/Melee/SwingLight", 4),
				new ModSoundStyle(nameof(TerrariaOverhaul), "Assets/Sounds/Items/Melee/SwingHeavy", 4),
				GetHeavyness(item),
				0.3f
			);
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
	}
}

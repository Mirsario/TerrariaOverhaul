using Microsoft.Xna.Framework;
using Terraria;
using TerrariaOverhaul.Common.Hooks.Items;
using TerrariaOverhaul.Common.ModEntities.NPCs;
using TerrariaOverhaul.Utilities;
using TerrariaOverhaul.Utilities.Extensions;

namespace TerrariaOverhaul.Common.ModEntities.Items.Components.Melee
{
	public sealed class ItemMeleeAttackAiming : ItemComponent, ICanMeleeCollideWithNPC
	{
		private Vector2 attackDirection;
		private float attackAngle;

		public int AttackId { get; private set; }

		public Vector2 AttackDirection {
			get => attackDirection;
			set {
				attackDirection = value;
				attackAngle = value.ToRotation();
			}
		}
		public float AttackAngle {
			get => attackAngle;
			set {
				attackAngle = value;
				attackDirection = value.ToRotationVector2();
			}
		}

		public override void UseAnimation(Item item, Player player)
		{
			AttackDirection = player.LookDirection();
			AttackId++;
		}

		public override void ModifyHitNPC(Item item, Player player, NPC target, ref int damage, ref float knockBack, ref bool crit)
		{
			// Make directional knockback work with melee.
			if (target.TryGetGlobalNPC(out NPCDirectionalKnockback npcKnockback)) {
				npcKnockback.SetNextKnockbackDirection(AttackDirection);
			}
		}

		public bool? CanMeleeCollideWithNPC(Item item, Player player, NPC target)
		{
			if (!Enabled) {
				return null;
			}

			float range = GetAttackRange(item, player);

			// Check arc collision
			return CollisionUtils.CheckRectangleVsArcCollision(target.getRect(), player.Center, AttackAngle, MathHelper.Pi * 0.5f, range);
		}

		public static float GetAttackRange(Item item, Player player)
		{
			float range = (item.Size * item.scale * 1.25f).Length();

			IModifyItemMeleeRange.Hook.Invoke(item, player, ref range);

			return range;
		}
	}
}

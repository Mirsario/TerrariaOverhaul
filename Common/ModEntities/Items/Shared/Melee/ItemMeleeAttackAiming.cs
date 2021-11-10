using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.Hooks.Items;
using TerrariaOverhaul.Utilities;
using TerrariaOverhaul.Utilities.Extensions;

namespace TerrariaOverhaul.Common.ModEntities.Items.Shared.Melee
{
	public sealed class ItemMeleeAttackAiming : GlobalItem, ICanMeleeCollideWithNPC
	{
		private Vector2 attackDirection;
		private float attackAngle;

		public bool Enabled { get; set; }
		public bool FlippedAttack { get; set; }
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

		public override bool InstancePerEntity => true;

		public override GlobalItem Clone(Item item, Item itemClone) => base.Clone(item, itemClone);

		public override void UseAnimation(Item item, Player player)
		{
			AttackDirection = player.LookDirection();
			AttackId++;
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

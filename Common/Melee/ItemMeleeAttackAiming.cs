using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.Damage;
using TerrariaOverhaul.Common.Hooks.Items;
using TerrariaOverhaul.Core.Debugging;
using TerrariaOverhaul.Core.ItemComponents;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.Melee;

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

	public override void ModifyHitNPC(Item item, Player player, NPC target, ref NPC.HitModifiers modifiers)
	{
		// Make directional knockback work with melee.
		if (target.TryGetGlobalNPC(out NPCDirectionalKnockback npcKnockback)) {
			npcKnockback.SetNextKnockbackDirection(AttackDirection);
		}
	}

	public bool? CanMeleeCollideWithNPC(Item item, Player player, NPC target, Rectangle itemRectangle)
	{
		if (!Enabled) {
			return null;
		}

		float range = GetAttackRange(item, player, itemRectangle);

		if (!Main.dedServ && DebugSystem.EnableDebugRendering) {
			DebugSystem.DrawRectangle(itemRectangle, Color.Purple);
		}

		// Check arc collision
		return CollisionUtils.CheckRectangleVsArcCollision(target.getRect(), player.Center, AttackAngle, MathHelper.Pi * 0.5f, range);
	}

	public static float GetAttackRange(Item item, Player player, Rectangle? itemRectangle = null)
	{
		Rectangle itemHitbox = GetMeleeHitbox(player, item);

		if (!Main.dedServ && DebugSystem.EnableDebugRendering) {
			DebugSystem.DrawRectangle(itemHitbox, Color.LightGoldenrodYellow);
		}

		float range = 0f;
		var playerCenter = player.Center;

		for (int i = 0; i < 4; i++) {
			var corner = i switch {
				0 => itemHitbox.TopLeft(),
				1 => itemHitbox.TopRight(),
				2 => itemHitbox.BottomRight(),
				3 => itemHitbox.BottomLeft(),
				_ => throw new InvalidOperationException()
			};
			float distanceToCorner = Vector2.Distance(playerCenter, corner);

			range = Math.Max(range, distanceToCorner);
		}

		IModifyItemMeleeRange.Invoke(item, player, ref range);

		return range;
	}

	public static Rectangle GetMeleeHitbox(Player player, Item item)
	{
		// Partially based on Player.ItemCheck_GetMeleeHitbox().
		// Would've been best to somehow make a shortened version of it via injections, but whatever.

		var playerCenter = player.Top;
		var itemRectangle = new Rectangle((int)playerCenter.X, (int)playerCenter.Y, 32, 32);

		if (!Main.dedServ) {
			Rectangle drawHitbox = Item.GetDrawHitbox(item.type, player);
			var size = drawHitbox.Size() + item.type switch {
				// Vanilla-derived hardcode.
				5094 or 5095 => new(-10, -10),
				5096 => new(-12, -12),
				5097 => new(-8, -8),
				_ => default
			};

			itemRectangle.Width = (int)size.X;
			itemRectangle.Height = (int)size.Y;
		}
		
		float adjustedItemScale = player.GetAdjustedItemScale(item);
		
		itemRectangle.Width = (int)(itemRectangle.Width * adjustedItemScale);
		itemRectangle.Height = (int)(itemRectangle.Height * adjustedItemScale);

		if (player.direction == -1) {
			itemRectangle.X -= itemRectangle.Width;
		}

		if (player.gravDir == 1f) {
			itemRectangle.Y -= itemRectangle.Height;
		}

		if (item.useStyle == ItemUseStyleID.Swing) {
			if (player.direction == -1) {
				itemRectangle.X -= (int)(itemRectangle.Width * 1.4 - itemRectangle.Width);
			}

			itemRectangle.Width = (int)(itemRectangle.Width * 1.4);
			itemRectangle.Y += (int)(itemRectangle.Height * 0.5 * (double)player.gravDir);
			itemRectangle.Height = (int)(itemRectangle.Height * 1.1);
		} else if (item.useStyle == ItemUseStyleID.Thrust) {
			if (player.direction == -1) {
				itemRectangle.X -= (int)(itemRectangle.Width * 1.4 - itemRectangle.Width);
			}

			itemRectangle.Width = (int)(itemRectangle.Width * 1.4);
			itemRectangle.Y += (int)(itemRectangle.Height * 0.6);
			itemRectangle.Height = (int)(itemRectangle.Height * 0.6);
			
			if (item.type == ItemID.Umbrella || item.type == ItemID.TragicUmbrella) {
				itemRectangle.Height += 14;
				itemRectangle.Width -= 10;

				if (player.direction == -1) {
					itemRectangle.X += 10;
				}
			}
		}

		bool dontAttackDummy = false;

		ItemLoader.UseItemHitbox(item, player, ref itemRectangle, ref dontAttackDummy);

		return itemRectangle;
	}
}

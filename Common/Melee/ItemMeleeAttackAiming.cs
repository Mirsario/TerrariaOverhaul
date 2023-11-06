using System;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.Damage;
using TerrariaOverhaul.Common.Hooks.Items;
using TerrariaOverhaul.Core.Debugging;
using TerrariaOverhaul.Core.ItemComponents;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.Melee;

public sealed class ItemMeleeAttackAiming : ItemComponent, ICanMeleeCollideWithNPC, IModifyItemNewProjectile
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

	public override void Load()
	{
		//On_Player.GetPointOnSwungItemPath += GetPointOnSwungItemPathDetour;

		IL_Player.ProcessHitAgainstNPC += ProcessHitAgainstNPCInjection;
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

	bool? ICanMeleeCollideWithNPC.CanMeleeCollideWithNPC(Item item, Player player, NPC target, Rectangle itemRectangle)
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

	void IModifyItemNewProjectile.ModifyShootProjectile(Player player, Item item, in IModifyItemNewProjectile.Args args, ref IModifyItemNewProjectile.Args result)
	{
		// Makes always horizontally-facing projectiles aimable.
		if (args.Source is EntitySource_ItemUse_WithAmmo && args.Velocity.Y == 0f && args.Velocity.X == player.direction) {
			if (item.TryGetGlobalItem(out ItemMeleeAttackAiming aiming)) {
				result.Velocity = aiming.AttackDirection;
			}
		}
	}

	public static float GetAttackRange(Item item, Player player, Rectangle? itemRectangle = null)
	{
		Rectangle itemHitbox = GetMeleeHitbox(player, item);

		if (!Main.dedServ && DebugSystem.EnableDebugRendering) {
			DebugSystem.DrawRectangle(itemHitbox, Color.LightGoldenrodYellow);
		}

		float sqrRange = 0f;
		var playerCenter = player.Center;

		for (int i = 0; i < 4; i++) {
			var corner = i switch {
				0 => itemHitbox.TopLeft(),
				1 => itemHitbox.TopRight(),
				2 => itemHitbox.BottomRight(),
				3 => itemHitbox.BottomLeft(),
				_ => throw new InvalidOperationException()
			};
			float sqrDistanceToCorner = Vector2.DistanceSquared(playerCenter, corner);

			sqrRange = Math.Max(sqrRange, sqrDistanceToCorner);
		}

		float range = sqrRange > 0f ? MathF.Sqrt(sqrRange) : 0f;

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

	/*
	private static void GetPointOnSwungItemPathDetour(On_Player.orig_GetPointOnSwungItemPath orig, Player player, float spriteWidth, float spriteHeight, float normalizedPointOnPath, float itemScale, out Vector2 location, out Vector2 outwardDirection)
	{
		orig(player, spriteWidth, spriteHeight, normalizedPointOnPath, itemScale, out location, out outwardDirection);

		if (DebugSystem.EnableDebugRendering) {
			DebugSystem.DrawCircle(location, 8f, Color.Turquoise, width: 3);
			DebugSystem.DrawLine(location, location + outwardDirection * 32f, Color.MediumTurquoise, width: 3);
		}
	}
	*/

	private static void ProcessHitAgainstNPCInjection(ILContext context)
	{
		var il = new ILCursor(context);

		// Skip a weird FieryGreatsword/Volcano block that culls collision for whatever reason.
		il.GotoNext(
			MoveType.After,
			i => i.MatchLdarg(1),
			i => i.MatchLdfld(typeof(Item), nameof(Item.type)),
			i => i.MatchLdcI4(ItemID.FieryGreatsword)
		);

		il.Emit(OpCodes.Pop);
		il.Emit(OpCodes.Ldc_I4, int.MinValue);
	}
}

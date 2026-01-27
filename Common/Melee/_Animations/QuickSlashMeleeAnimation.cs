// Copyright (c) 2020-2026 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using TerrariaOverhaul.Common.Charging;
using TerrariaOverhaul.Common.Hooks.Items;
using TerrariaOverhaul.Utilities.Terraria;
using TerrariaOverhaul.Utilities.Xna;

namespace TerrariaOverhaul.Common.Melee;

/// <summary>
/// Quick swing that lasts 1/2 of the use animation time.
/// Affects gameplay.
/// </summary>
internal class QuickSlashMeleeAnimation : MeleeAnimation, ICanDoMeleeDamage, IModifyItemNewProjectile
{
	public bool IsAttackFlipped { get; set; }
	public bool FlipAttackEachSwing { get; set; }
	public bool AnimateLegs { get; set; }

	public override float GetItemRotation(Player player, Item item)
	{
		float baseAngle;

		if (item.TryGetGlobalItem(out ItemMeleeAttackAiming meleeAiming)) {
			baseAngle = meleeAiming.AttackAngle;
		} else {
			baseAngle = 0f;
		}

		if (item.TryGetGlobalItem(out ItemPowerAttacks powerAttacks) && powerAttacks.Enabled && powerAttacks.IsCharging) {
			baseAngle -= MathHelper.PiOver2 * 0.25f * player.direction;
		}

		float step = 1f - MathHelper.Clamp(player.itemAnimation / (float)player.itemAnimationMax, 0f, 1f);
		int dir = player.direction * (IsAttackFlipped ? -1 : 1);

		float minValue = baseAngle - (MathHelper.PiOver2 * 1.25f * dir);
		float maxValue = baseAngle + (MathHelper.PiOver2 * 1.00f * dir);

		var animation = new Gradient<float>(
			(0.000f, 0.000f),
			(0.100f, 0.000f),
			(0.150f, 0.125f),
			(0.151f, 0.800f),
			(0.500f, 1.000f),
			(0.800f, 0.800f),
			(1.000f, 0.800f)
		);

		return MathHelper.Lerp(minValue, maxValue, animation.GetValue(step));
	}

	// Direction switching
	public override void UseAnimation(Item item, Player player)
	{
		base.UseAnimation(item, player);

		if (!Enabled || !FlipAttackEachSwing) {
			return;
		}

		var powerAttacks = item.GetGlobalItem<ItemPowerAttacks>();

		if ((!powerAttacks.Enabled || !powerAttacks.PowerAttack) && item.TryGetGlobalItem(out ItemMeleeAttackAiming aiming)) {
			IsAttackFlipped = aiming.AttackId % 2 != 0;
		}
	}

	// Leg framing
	protected override void ApplyAnimation(Item item, Player player)
	{
		base.ApplyAnimation(item, player);

		if (!Enabled || !AnimateLegs) {
			return;
		}

		var aiming = item.GetGlobalItem<ItemMeleeAttackAiming>();

		if (player.velocity.Y == 0f && player.KeyDirection().X == 0f) {
			if (Math.Abs(aiming.AttackDirection.X) > 0.5f) {
				player.legFrame = (IsAttackFlipped ? PlayerFrames.Walk8 : PlayerFrames.Jump).ToRectangle();
			} else {
				player.legFrame = PlayerFrames.Walk13.ToRectangle();
			}
		}
	}

	bool ICanDoMeleeDamage.CanDoMeleeDamage(Item item, Player player)
	{
		if (!Enabled) {
			return true;
		}

		// Damage will only be applied during the first half of the use.
		// The second half is a cooldown, and the animations reflect that.
		return player.itemAnimation >= player.itemAnimationMax / 2;
	}

	void IModifyItemNewProjectile.ModifyShootProjectile(Player player, Item item, in IModifyItemNewProjectile.Args args, ref IModifyItemNewProjectile.Args result)
	{
		if (args.Source is EntitySource_ItemUse_WithAmmo
			// For horizontally-facing projectiles
			&& args.Velocity.Y == 0f
			&& args.Velocity.X == player.direction
			// That pass rotation direction as AI0
			&& args.AI0 == player.direction * player.gravDir
			// Whenever the slash animation is flipped
			&& item.TryGetGlobalItem(out QuickSlashMeleeAnimation slashAnimation)
			&& slashAnimation.IsAttackFlipped
		) {
			// Flip the rotation direction
			result.AI0 = -args.AI0;
		}
	}
}

using System;
using Microsoft.Xna.Framework;
using Terraria;
using TerrariaOverhaul.Common.Charging;
using TerrariaOverhaul.Common.Hooks.Items;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.Melee;

/// <summary>
/// Quick swing that lasts 1/2 of the use animation time.
/// Affects gameplay.
/// </summary>
public class QuickSlashMeleeAnimation : MeleeAnimation, ICanDoMeleeDamage
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

		float step = 1f - MathHelper.Clamp(player.itemAnimation / (float)player.itemAnimationMax, 0f, 1f);
		int dir = player.direction * (IsAttackFlipped ? -1 : 1);

		float minValue = baseAngle - MathHelper.PiOver2 * 1.25f;
		float maxValue = baseAngle + MathHelper.PiOver2 * 1.0f;

		if (dir < 0) {
			Utils.Swap(ref minValue, ref maxValue);
		}

		var animation = new Gradient<float>(
			(0.0f, minValue),
			(0.1f, minValue),
			(0.15f, MathHelper.Lerp(minValue, maxValue, 0.125f)),
			(0.151f, MathHelper.Lerp(minValue, maxValue, 0.8f)),
			(0.5f, maxValue),
			(0.8f, MathHelper.Lerp(minValue, maxValue, 0.8f)),
			(1.0f, MathHelper.Lerp(minValue, maxValue, 0.8f))
		);

		return animation.GetValue(step);
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
	public override void UseItemFrame(Item item, Player player)
	{
		base.UseItemFrame(item, player);

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

	public bool CanDoMeleeDamage(Item item, Player player)
	{
		if (!Enabled) {
			return true;
		}

		// Damage will only be applied during the first half of the use.
		// The second half is a cooldown, and the animations reflect that.
		return player.itemAnimation >= player.itemAnimationMax / 2;
	}
}

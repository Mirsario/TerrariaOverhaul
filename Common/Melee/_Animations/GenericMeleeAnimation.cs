using Microsoft.Xna.Framework;
using Terraria;

namespace TerrariaOverhaul.Common.Melee;

public class GenericMeleeAnimation : MeleeAnimation
{
	public override float GetItemRotation(Player player, Item item)
	{
		if (!item.TryGetGlobalItem(out ItemMeleeAttackAiming aimableAttacks)) {
			return 0f;
		}

		float baseAngle = aimableAttacks.AttackAngle;
		float step = 1f - MathHelper.Clamp(player.itemAnimation / (float)player.itemAnimationMax, 0f, 1f);

		float minValue = baseAngle - MathHelper.PiOver2;
		float maxValue = baseAngle + MathHelper.PiOver2;

		if (player.direction < 0) {
			Utils.Swap(ref minValue, ref maxValue);
		}

		return MathHelper.Lerp(minValue, maxValue, step);
	}
}

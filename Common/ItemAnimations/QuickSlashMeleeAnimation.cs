using Microsoft.Xna.Framework;
using Terraria;
using TerrariaOverhaul.Common.ModEntities.Items.Components.Melee;
using TerrariaOverhaul.Utilities.DataStructures;

namespace TerrariaOverhaul.Common.ItemAnimations
{
	public class QuickSlashMeleeAnimation : MeleeAnimation
	{
		public override float GetItemRotation(Item item, Player player)
		{
			if (!item.TryGetGlobalItem(out ItemMeleeAttackAiming meleeAiming, false)) {
				return 0f;
			}

			float baseAngle = meleeAiming.AttackAngle;
			float step = 1f - MathHelper.Clamp(player.itemAnimation / (float)player.itemAnimationMax, 0f, 1f);
			int dir = player.direction * (meleeAiming.FlippedAttack ? -1 : 1);

			float minValue = baseAngle - (MathHelper.PiOver2 * 1.25f);
			float maxValue = baseAngle + (MathHelper.PiOver2 * 1.0f);

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
	}
}

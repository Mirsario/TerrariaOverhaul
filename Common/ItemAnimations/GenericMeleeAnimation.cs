using Microsoft.Xna.Framework;
using Terraria;
using TerrariaOverhaul.Common.ModEntities.Items.Overhauls.Generic;

namespace TerrariaOverhaul.Common.ItemAnimations
{
	public class GenericMeleeAnimation : MeleeAnimation
	{
		public override float GetItemRotation(Item item, Player player)
		{
			if(!item.TryGetGlobalItem(out MeleeWeapon meleeWeapon, false)) {
				return 0f;
			}

			float baseAngle = meleeWeapon.AttackAngle;
			float step = 1f - MathHelper.Clamp(player.itemAnimation / (float)player.itemAnimationMax, 0f, 1f);

			float minValue = baseAngle - MathHelper.PiOver2;
			float maxValue = baseAngle + MathHelper.PiOver2;

			if(player.direction < 0) {
				Utils.Swap(ref minValue, ref maxValue);
			}

			return MathHelper.Lerp(minValue, maxValue, step);
		}
	}
}

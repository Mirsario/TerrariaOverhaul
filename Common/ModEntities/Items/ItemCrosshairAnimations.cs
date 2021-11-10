using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.Systems.Crosshairs;
using TerrariaOverhaul.Common.Systems.Time;
using TerrariaOverhaul.Utilities.Extensions;

namespace TerrariaOverhaul.Common.ModEntities.Items
{
	[Autoload(Side = ModSide.Client)]
	public sealed class ItemCrosshairAnimations : GlobalItem
	{
		public override bool? UseItem(Item item, Player player)
		{
			if (!player.IsLocal() || !CrosshairSystem.ShowCrosshair) {
				return null;
			}

			int useTime = CombinedHooks.TotalUseTime(item.useTime, player, item);
			float useTimeInSeconds = useTime * TimeSystem.LogicDeltaTime;

			CrosshairSystem.AddImpulse(7f, useTimeInSeconds);
			CrosshairSystem.AddImpulse(0f, useTimeInSeconds * 0.5f, color: Color.White);

			return null;
		}

		public override void UseAnimation(Item item, Player player)
		{
			if (!player.IsLocal() || !CrosshairSystem.ShowCrosshair) {
				return;
			}

			const int MinTime = 25;

			if (item.useAnimation > MinTime) {
				int useAnimation = CombinedHooks.TotalAnimationTime(item.useAnimation, player, item);

				if (useAnimation > MinTime) {
					CrosshairSystem.AddImpulse(10f, useAnimation * TimeSystem.LogicDeltaTime, autoRotation: true);
				}
			}
		}
	}
}

using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Core.ItemComponents;
using TerrariaOverhaul.Core.Time;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.Crosshairs;

[Autoload(Side = ModSide.Client)]
public sealed class ItemCrosshairController : ItemComponent
{
	public override void SetDefaults(Item item)
	{
		static bool CheckItem(Item item)
		{
			if (item.shoot <= ProjectileID.None) {
				return false;
			}

			if (item.buffType > 0) {
				return false;
			}

			if (item.pick > 0 || item.axe > 0 || item.hammer > 0) {
				return false;
			}

			return false;
		}

		Enabled |= CheckItem(ContentSamples.ItemsByType.TryGetValue(item.type, out var baseItem) ? baseItem : item);
	}

	public override bool? UseItem(Item item, Player player)
	{
		if (!Enabled || !player.IsLocal()) {
			return null;
		}

		if (item.channel) {
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
		if (!Enabled || !player.IsLocal()) {
			return;
		}

		if (item.channel) {
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

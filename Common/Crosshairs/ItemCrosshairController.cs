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
	public CrosshairEffects? UseItemEffects { get; set; }
	public CrosshairEffects? UseAnimationEffects { get; set; }

	public override void OnEnabled(Item item)
	{
		// Should be filled prior to ItemsByType
		if (!ContentSamples.ProjectilesByType.TryGetValue(item.shoot, out var projectile)) {
			return;
		}

		switch (projectile.aiStyle) {
			case ProjAIStyleID.Yoyo:
				break;
			default:
				UseItemEffects = new CrosshairEffects {
					Offset = (7.0f, 1.0f),
					InnerColor = (Color.White, 0.5f),
				};
				break;
		}

		switch (projectile.aiStyle) {
			// Don't create animation effects on yoyo use
			case ProjAIStyleID.Yoyo:
			case ProjAIStyleID.Spear:
				return;
			case int _ when item.useAnimation >= 20:
				UseAnimationEffects = new CrosshairEffects {
					Offset = (9.0f, 1.0f),
					Rotation = (
					item.useAnimation switch {
						<= 30 => MathHelper.PiOver2,
						<= 60 => MathHelper.Pi,
						_ => MathHelper.TwoPi,
					},
					1.0f
				),
				};
				break;
		}
	}

	public override void SetDefaults(Item item)
	{
		static bool CheckItem(Item item)
		{
			// Only apply to items that fire projectiles.
			if (item.shoot <= ProjectileID.None) {
				return false;
			}

			// Ignore summons and anything that gives buffs.
			if (item.buffType > 0) {
				return false;
			}

			// Ignore drills, chainsaws, and jackhammers.
			if (item.pick > 0 || item.axe > 0 || item.hammer > 0) {
				return false;
			}

			return true;
		}

		if (!Enabled && CheckItem(ContentSamples.ItemsByType.TryGetValue(item.type, out var baseItem) ? baseItem : item)) {
			SetEnabled(item, true);
		}
	}

	public override bool? UseItem(Item item, Player player)
	{
		if (!Enabled || !player.IsLocal()) {
			return null;
		}

		if (UseItemEffects is CrosshairEffects effects) {
			int useTime = CombinedHooks.TotalUseTime(item.useTime, player, item);
			float useTimeInSeconds = useTime * TimeSystem.LogicDeltaTime;

			effects.Rotation.Value *= -player.direction;

			CrosshairSystem.AddImpulse(effects, useTimeInSeconds);
		}

		return null;
	}

	public override void UseAnimation(Item item, Player player)
	{
		if (!Enabled || !player.IsLocal()) {
			return;
		}

		if (UseAnimationEffects is CrosshairEffects effects) {
			int useAnimation = CombinedHooks.TotalAnimationTime(item.useAnimation, player, item);
			float useAnimationInSeconds = useAnimation * TimeSystem.LogicDeltaTime;

			effects.Rotation.Value *= -player.direction;

			CrosshairSystem.AddImpulse(effects, useAnimationInSeconds);
		}
	}
}

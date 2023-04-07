using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Core.ItemComponents;
using TerrariaOverhaul.Core.Time;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.Camera;

[Autoload(Side = ModSide.Client)]
public sealed class ItemUseScreenShake : ItemComponent
{
	public ScreenShake ScreenShake { get; set; } = new(0.2f, 0.25f);

	public override void OnEnabled(Item item)
	{
		float useTimeInSeconds = item.useTime * TimeSystem.LogicDeltaTime;
		float useAnimInSeconds = item.useAnimation * TimeSystem.LogicDeltaTime;

		float power = MathHelper.Lerp(0.0f, 0.5f, MathUtils.Clamp01(MathHelper.Lerp(useTimeInSeconds * 2.0f, 0.2f, 0.5f)));
		float length = MathUtils.Clamp(MathHelper.Lerp(useAnimInSeconds, 0.2f, 0.5f), 0.1f, 1.0f);

		ScreenShake = new ScreenShake(power, length);
	}

	public override void SetDefaults(Item item)
	{
		static bool CheckItem(Item item)
		{
			// Only apply to items that fire projectiles.
			if (item.shoot <= ProjectileID.None || item.shoot >= ProjectileLoader.ProjectileCount) {
				return false;
			}

			// Ignore all melee items, since this is about UseItem and not UseAnimation.
			if (item.CountsAsClass<MeleeDamageClass>()) {
				return false;
			}

			// Ignore summons and anything that gives buffs.
			if (item.buffType > 0) {
				return false;
			}

			// Ignore channeled items
			if (item.channel) {
				return false;
			}

			var projectile = ContentSampleUtils.GetProjectile(item.shoot);

			// Ignore spears
			if (projectile.aiStyle == ProjAIStyleID.Spear) {
				return false;
			}

			// Ignore items that don't deal damage, with exceptions.
			if (item.damage <= 0) {
				// Water and slime guns are excepted
				if (item.shoot is ProjectileID.WaterGun or ProjectileID.SlimeGun) {
					return true;
				}

				return false;
			}

			return true;
		}

		if (!Enabled && CheckItem(ContentSampleUtils.TryGetItem(item.type, out var baseItem) ? baseItem : item)) {
			SetEnabled(item, true);
		}
	}

	public override bool? UseItem(Item item, Player player)
	{
		if (Enabled) {
			var screenShake = ScreenShake;

			ScreenShakeSystem.New(screenShake, player.Center);
		}

		return base.UseItem(item, player);
	}
}

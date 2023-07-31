using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.Charging;
using TerrariaOverhaul.Core.Time;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.Archery;

[Autoload(Side = ModSide.Client)]
public class ArrowPlayerDrawLayer : PlayerDrawLayer
{
	public override Position GetDefaultPosition()
		=> new AfterParent(PlayerDrawLayers.HeldItem);

	public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
		=> true;

	protected override void Draw(ref PlayerDrawSet drawInfo)
	{
		var player = drawInfo.drawPlayer;

		// Don't render as after-image
		if (drawInfo.shadow != 0f) {
			return;
		}

		// Only render while a use is in progress
		if (player.itemAnimation <= 0 || player.itemAnimationMax <= 0) {
			return;
		}

		// Must be holding an arrow-based weapon
		if (player.HeldItem is not { IsAir: false } weapon || weapon.useAmmo != AmmoID.Arrow) {
			return;
		}

		// Must be charging the weapon
		float chargeProgress;

		if (weapon.TryGetGlobalItem(out ItemPowerAttacks powerAttacks) && powerAttacks.IsCharging) {
			chargeProgress = powerAttacks.Charge.Progress;
		} else if (weapon.TryGetGlobalItem(out ItemPrimaryUseCharging primaryCharging) && primaryCharging.Charge.Active) {
			chargeProgress = primaryCharging.Charge.Progress;
		} else {
			return;
		}

		var ammo = player.ChooseAmmo(weapon);

		if (ammo.IsAir || ammo.shoot <= ProjectileID.None || ammo.shoot >= ProjectileLoader.ProjectileCount) {
			return;
		}

		if (!ContentSamples.ProjectilesByType.TryGetValue(ammo.shoot, out var projectile)) {
			return;
		}

		// Attack info
		float itemRotation = player.itemRotation + (player.direction < 0 ? MathHelper.Pi : 0f);
		Vector2 itemDirection = itemRotation.ToRotationVector2();

		// Texture info
		Main.instance.LoadProjectile(projectile.type);

		var projectileTexture = TextureAssets.Projectile[projectile.type].Value;
		var frame = new SpriteFrame(1, 1);
		var sourceRectangle = frame.GetSourceRectangle(projectileTexture);

		// Animation
		const float StartOffset = 32f;
		const float EndOffset = 14f;

		float positionOffsetEasing = 1f - MathF.Pow(1f - chargeProgress, 2f);
		float positionOffset = MathHelper.Lerp(StartOffset, EndOffset, positionOffsetEasing);
		float rotationOffsetIntensity = Math.Min(1f, MathF.Pow(chargeProgress, 5f) + 0.1f);
		float rotationOffset = MathF.Sin(TimeSystem.RenderTime * 50f) * MathHelper.ToRadians(7.5f) * rotationOffsetIntensity;

		// Drawing info
		var position = player.Center + itemDirection * positionOffset;
		float rotation = itemRotation - MathHelper.PiOver2 * player.direction + rotationOffset;
		Vector2 origin = sourceRectangle.Size() * 0.5f;
		float scale = 1.0f;
		var effect = player.direction > 0 ? SpriteEffects.FlipVertically : SpriteEffects.None;
		var color = Lighting.GetColor(position.ToTileCoordinates());

		// Drawing
		drawInfo.DrawDataCache.Add(new DrawData(projectileTexture, position - Main.screenPosition, sourceRectangle, color, rotation, origin, scale, effect, 0));
	}
}

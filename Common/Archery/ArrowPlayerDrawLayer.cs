using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.Charging;
using TerrariaOverhaul.Common.TextureColors;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.Melee;

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

		// Only render while a use is in progress
		if (player.itemAnimation <= 0 || player.itemAnimationMax <= 0) {
			return;
		}

		// Must be holding an arrow-based weapon
		if (player.HeldItem is not { IsAir: false } weapon || weapon.useAmmo != AmmoID.Arrow) {
			return;
		}

		// Must be charging the weapon
		if (weapon.TryGetGlobalItem<ItemPowerAttacks>(out var powerAttacks) && !powerAttacks.IsCharging) {
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
		float chargeProgress = powerAttacks.Charge.Progress;
		var firingDirection = player.LookDirection();
		float firingAngle = firingDirection.ToRotation();

		// Texture info
		Main.instance.LoadProjectile(projectile.type);

		var projectileTexture = TextureAssets.Projectile[projectile.type].Value;
		var frame = new SpriteFrame(1, 1);
		var sourceRectangle = frame.GetSourceRectangle(projectileTexture);

		// Animation
		const float StartOffset = 32f;
		const float EndOffset = 12f;

		float animationEasing = 1f - MathF.Pow(1f - chargeProgress, 2f);
		float arrowOffset = MathHelper.Lerp(StartOffset, EndOffset, animationEasing);

		// Drawing info
		var position = player.Center + firingDirection * arrowOffset;
		float rotation = firingAngle - MathHelper.PiOver2 * player.direction;
		Vector2 origin = sourceRectangle.Size() * 0.5f;
		float scale = 1.0f;
		var effect = player.direction > 0 ? SpriteEffects.FlipVertically : SpriteEffects.None;
		var color = Lighting.GetColor(position.ToTileCoordinates());

		// Drawing
		drawInfo.DrawDataCache.Add(new DrawData(projectileTexture, position - Main.screenPosition, sourceRectangle, color, rotation, origin, scale, effect, 0));
	}
}

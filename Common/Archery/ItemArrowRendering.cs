using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.Charging;
using TerrariaOverhaul.Core.ItemComponents;
using TerrariaOverhaul.Core.Time;

namespace TerrariaOverhaul.Common.Archery;

[Autoload(Side = ModSide.Client)]
public sealed class ItemArrowRendering : ItemComponent
{
	public bool Visible { get; private set; }
	public byte ArrowCount { get; private set; } = 1;
	public float AnimationProgress { get; private set; }
	public Projectile? AmmoProjectileSample { get; private set; }

	public override void SetDefaults(Item item)
	{
		// Hardcoded for now.
		if (item.type == ItemID.Tsunami) {
			ArrowCount = 5;
		}
	}

	public override void HoldItem(Item item, Player player)
	{
		Visible = Enabled && UpdateIsVisible(item, player);
	}

	private bool UpdateIsVisible(Item item, Player player)
	{
		// Only render while a use is in progress
		if (!player.ItemAnimationActive) {
			return false;
		}

		// Must be charging the weapon
		if (item.TryGetGlobalItem(out ItemPowerAttacks powerAttacks) && powerAttacks.IsCharging) {
			AnimationProgress = powerAttacks.Charge.Progress;
		} else if (item.TryGetGlobalItem(out ItemPrimaryUseCharging primaryCharging) && primaryCharging.Charge.Active) {
			AnimationProgress = primaryCharging.Charge.Progress;
		} else {
			return false;
		}

		// Update ammo kind only at the beginning of the animation.
		if (!Visible) {
			player.PickAmmo(item, out int projectileType, out _, out _, out _, out _, dontConsume: true);

			if (!ContentSamples.ProjectilesByType.TryGetValue(projectileType, out var projectile) || projectile == null) {
				return false;
			}

			AmmoProjectileSample = projectile;
		}

		return true;
	}
}

[Autoload(Side = ModSide.Client)]
public sealed class ArrowPlayerDrawLayer : PlayerDrawLayer
{
	public override Position GetDefaultPosition()
		=> new AfterParent(PlayerDrawLayers.HeldItem);

	public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
	{
		// Don't render as after-image
		return drawInfo.shadow == 0f;
	}

	protected override void Draw(ref PlayerDrawSet drawInfo)
	{
		var player = drawInfo.drawPlayer;

		// Must be holding a weapon with the ItemArrowRendering component.
		if (player.HeldItem is not { IsAir: false } weapon
		|| !weapon.TryGetGlobalItem(out ItemArrowRendering arrowRendering)
		|| arrowRendering is not { Enabled: true, Visible: true, AmmoProjectileSample: Projectile projectile }) {
			return;
		}

		// Attack info
		float itemRotation = player.itemRotation + (player.direction < 0 ? MathHelper.Pi : 0f);
		Vector2 itemDirection = itemRotation.ToRotationVector2();

		// Texture info
		Main.instance.LoadProjectile(projectile.type);

		var projectileTexture = TextureAssets.Projectile[projectile.type].Value;
		var frame = new SpriteFrame(1, (byte)Main.projFrames[projectile.type]);
		var sourceRectangle = frame.GetSourceRectangle(projectileTexture);

		// Animation
		const float StartOffset = 32f;
		const float EndOffset = 14f;

		float animationProgress = arrowRendering.AnimationProgress;
		float positionOffsetEasing = 1f - MathF.Pow(1f - animationProgress, 2f);
		var positionOffset = itemDirection * MathHelper.Lerp(StartOffset, EndOffset, positionOffsetEasing);
		float rotationOffsetIntensity = Math.Min(1f, MathF.Pow(animationProgress, 5f) + 0.1f);
		float rotationOffset = MathF.Sin(TimeSystem.RenderTime * 50f) * MathHelper.ToRadians(7.5f) * rotationOffsetIntensity;

		for (int i = 0; i < arrowRendering.ArrowCount; i++) {
			int arrowStep = (int)MathF.Ceiling(i * 0.5f) * (i % 2 == 0 ? 1 : -1);
			var itemDirectionRight = itemDirection.RotatedBy(MathHelper.PiOver2);
			var arrowOffset = (itemDirectionRight * arrowStep * 8f) + (itemDirection * MathF.Abs(arrowStep) * -2f);

			// Drawing info
			var position = player.Center + positionOffset + arrowOffset;
			float rotation = itemRotation - MathHelper.PiOver2 * player.direction + rotationOffset;
			Vector2 origin = sourceRectangle.Size() * 0.5f;
			float scale = 1.0f;
			var effect = player.direction > 0 ? SpriteEffects.FlipVertically : SpriteEffects.None;
			var color = Lighting.GetColor(position.ToTileCoordinates());

			// Drawing
			drawInfo.DrawDataCache.Add(new DrawData(projectileTexture, position - Main.screenPosition, sourceRectangle, color, rotation, origin, scale, effect, 0));
		}
	}
}

using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ModLoader;
using TerrariaOverhaul.Core.Debugging;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.Guns;

[Autoload(Side = ModSide.Client)]
public class MuzzleflashPlayerDrawLayer : PlayerDrawLayer
{
	private static Dictionary<int, Vector2>? weaponBarrelEndPositions;

	public override Position GetDefaultPosition()
		=> new AfterParent(PlayerDrawLayers.HeldItem);

	public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
		=> true;

	protected override void Draw(ref PlayerDrawSet drawInfo)
	{
		var player = drawInfo.drawPlayer;
		var item = player.HeldItem;

		// Return if no muzzleflash is currently active.
		if (item?.IsAir != false || !item.TryGetGlobalItem(out ItemMuzzleflashes muzzleflashes) || !muzzleflashes.IsVisible) {
			return;
		}

		// Make sure that the item texture is loaded.
		Main.instance.LoadItem(item.type);

		var itemTexture = TextureAssets.Item[item.type].Value;
		var weaponBarrelEnd = GetWeaponBarrelEndPosition(item.type, itemTexture);

		// Find the DrawData of the held item.
		if (drawInfo.DrawDataCache.FindIndex(d => d.texture == itemTexture) is not (>= 0 and int itemDrawDataIndex)) {
			return;
		}

		var weaponData = drawInfo.DrawDataCache[itemDrawDataIndex];
		var weaponPosition = weaponData.position;
		var weaponFixedOrigin = player.direction > 0 ? weaponData.origin : Vector2.UnitX * weaponData.texture.Width - weaponData.origin;

		if (DebugSystem.EnableDebugRendering) {
			DebugSystem.DrawCircle(weaponPosition + Main.screenPosition, 4f, Color.White);
		}

		var originOffset = new Vector2(
			(weaponBarrelEnd.X - weaponFixedOrigin.X) * player.direction,
			-weaponFixedOrigin.Y * player.direction + weaponBarrelEnd.Y
		) * item.scale;

		var muzzleflashPosition = weaponPosition + originOffset.RotatedBy(player.itemRotation);

		var style = muzzleflashes.CurrentStyle;
		var colors = muzzleflashes.Colors;
		var segmentFrames = style.SegmentFrames;
		int segmentInsertionIndex = itemDrawDataIndex;

		int colorIndex = 0;
		float animationProgress = muzzleflashes.AnimationProgress;

		// Render segments
		for (int i = 0; i < segmentFrames.Length; i++) {
			float fadeoutPoint = (i + 1) / (float)segmentFrames.Length;

			if (animationProgress >= fadeoutPoint) {
				continue;
			}

			if (style.Texture.Value is not Texture2D texture) {
				continue;
			}

			var frame = segmentFrames[i];
			var srcRect = frame.GetSourceRectangle(texture);
			var color = colors[colorIndex++];

			var muzzleflashOrigin = new Vector2(player.direction < 0 ? srcRect.Width : 0f, srcRect.Height * 0.5f);
			var flashData = new DrawData(texture, muzzleflashPosition, srcRect, color, player.itemRotation, muzzleflashOrigin, 1f, drawInfo.itemEffect, 0);

			drawInfo.DrawDataCache.Insert(segmentInsertionIndex++, flashData);
		}

		if (muzzleflashes.LightColor.LengthSquared() > 0f) {
			Lighting.AddLight(muzzleflashPosition, muzzleflashes.LightColor);
		}
	}

	/// <summary> Tries to calculate the center of the end of a weapon's barrel based on its texture. </summary>
	private static Vector2 GetWeaponBarrelEndPosition(int type, Texture2D texture)
	{
		weaponBarrelEndPositions ??= new();

		if (weaponBarrelEndPositions.TryGetValue(type, out var result)) {
			return result;
		}

		var surface = new Surface<Color>(texture.Width, texture.Height);

		texture.GetData(surface.Data);

		var columnPoints = new List<Vector2>();

		for (int x = surface.Width - 1; x >= 0; x--) {
			bool columnIsEmpty = true;

			for (int y = 0; y < surface.Height; y++) {
				if (surface[x, y].A > 0) {
					columnIsEmpty = false;

					columnPoints.Add(new Vector2(x, y));
				}
			}

			if (!columnIsEmpty) {
				break;
			}
		}

		result = default;

		if (columnPoints.Count > 0) {
			foreach (var value in columnPoints) {
				result += value;
			}

			result /= columnPoints.Count;
		}

		weaponBarrelEndPositions[type] = result;

		return result;
	}
}

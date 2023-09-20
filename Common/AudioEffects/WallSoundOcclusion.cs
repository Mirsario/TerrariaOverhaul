using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Core.AudioEffects;
using TerrariaOverhaul.Core.Debugging;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.AudioEffects;

//TODO: Rewrite to dynamically detect sounds as ones happening inside or outside, occlude if not the same.
public sealed class WallSoundOcclusion : ModSystem
{
	private static readonly HashSet<SoundStyle> soundStyles = new() {
		SoundID.Bird,
		SoundID.Thunder,
	};

	public static float OcclusionFactor { get; private set; }

	public override void Load()
	{
		AudioEffectsSystem.OnSoundUpdate += ApplyOcclusionToSounds;
	}

	public override void PostUpdateWorld()
	{
		if (Main.LocalPlayer is not Player { active: true } player) {
			return;
		}

		const int FloodFillExtents = 5;
		const float RequiredWallRatio = 0.4f;

		Vector2Int areaCenter = player.Center.ToTileCoordinates();
		var areaRectangle = new Rectangle(areaCenter.X, areaCenter.Y, 0, 0).Extended(FloodFillExtents);

		int maxTiles = areaRectangle.Width * areaRectangle.Height;
		int requiredWallTiles = (int)(maxTiles * RequiredWallRatio);
		int numWalls = 0;

		foreach (var p in new GeometryUtils.FloodFill(areaCenter, areaRectangle.ClampTileCoordinates())) {
			var (x, y) = p.Point;
			Tile tile = Main.tile[x, y];

			bool isPointFree = p.IsPointFree = !tile.HasTile || !Main.tileSolid[tile.TileType] || Main.tileSolidTop[tile.TileType] || tile.BlockType != BlockType.Solid;

			p.IsPointFree = isPointFree;

			if (isPointFree && (tile.WallType > 0 || y >= Main.worldSurface)) {
				numWalls++;

				if (DebugSystem.EnableDebugRendering) {
					DebugSystem.DrawRectangle(new Rectangle(x * 16, y * 16, 16, 16), Color.Red, 1);
				}

				if (numWalls >= requiredWallTiles && !DebugSystem.EnableDebugRendering) {
					break;
				}
			}
		}

		OcclusionFactor = MathHelper.Clamp(numWalls / (float)requiredWallTiles, 0f, 1f);
	}

	public static void SetEnabledForSoundStyle(SoundStyle soundStyle, bool enabled)
	{
		if (enabled) {
			soundStyles.Add(soundStyle);
		} else {
			soundStyles.Remove(soundStyle);
		}
	}

	private static void ApplyOcclusionToSounds(Span<AudioEffectsSystem.SoundData> sounds)
	{
		float occlusionFactor = OcclusionFactor;

		for (int i = 0; i < sounds.Length; i++) {
			ref var data = ref sounds[i];

			if (soundStyles.Contains(data.SoundStyle)) {
				data.Parameters.LowPassFiltering += occlusionFactor;
			}
		}
	}
}

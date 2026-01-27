// Copyright (c) 2020-2026 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.Camera;
using TerrariaOverhaul.Core.AudioEffects;
using TerrariaOverhaul.Core.Debugging;
using TerrariaOverhaul.Utilities.Terraria;
using TerrariaOverhaul.Utilities.Xna;

namespace TerrariaOverhaul.Common.AudioEffects;

internal sealed class TileSoundOcclusion : ModSystem
{
	private static readonly HashSet<SoundStyle> excludedSoundStyles = [];

	public static float OcclusionFactor { get; private set; }

	public static int MaxOccludingTiles { get; set; } = 15;
	public static int MaxDistanceForOcclusionChecks { get; set; } = 4096;

	public override void Load()
	{
		AudioEffectsSystem.OnSoundUpdate += ApplyOcclusionToSounds;
	}

	public static void SetEnabledForSoundStyle(in SoundStyle soundStyle, bool enabled)
	{
		if (enabled) {
			excludedSoundStyles.Remove(soundStyle);
		} else {
			excludedSoundStyles.Add(soundStyle);
		}
	}

	private static void ApplyOcclusionToSounds(Span<AudioEffectsSystem.SoundData> sounds)
	{
		var listenerPos = CameraSystem.ScreenCenter;
		var listenerTilePos = CameraSystem.ScreenCenter.ToTileCoordinates();
		float maxSqrDistance = MaxDistanceForOcclusionChecks * MaxDistanceForOcclusionChecks;

		foreach (ref var data in sounds) {
			if (data.TrackedSound?.TryGetTarget(out var activeSound) == true && activeSound.Position is Vector2 position) {
				if (excludedSoundStyles.Contains(data.SoundStyle))
					continue;

				// Halt if too far away, else the line check will be too expensive.
				if (position.DistanceSQ(listenerPos) >= maxSqrDistance)
					continue;

				float occlusion = CalculateSoundOcclusion(listenerTilePos, position.ToTileCoordinates());
				data.Parameters.LowPassFiltering += occlusion;
			}
		}
	}

	private static float CalculateSoundOcclusion(Vector2Int center, Vector2Int position)
	{
		int occludingTiles = 0;

		foreach (var point in new GeometryUtils.BresenhamLine(center, position)) {
			if (!Main.tile.TryGet(point, out var tile))
				break;

			bool solid = tile.HasTile && Main.tileSolid[tile.TileType];

			if (DebugSystem.EnableDebugRendering)
				DebugSystem.DrawRectangle(new Rectangle(point.X, point.Y, 1, 1).ToWorldCoordinates(), solid ? Color.Orange : Color.GreenYellow, 1);

			if (solid) {
				occludingTiles += 1;
				if (occludingTiles >= MaxOccludingTiles)
					break;
			}
		}

		return occludingTiles / (float)MaxOccludingTiles;
	}
}

using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.Camera;
using TerrariaOverhaul.Core.AudioEffects;
using TerrariaOverhaul.Core.Debugging;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.AudioEffects;

public sealed class TileSoundOcclusion : ModSystem
{
	public static float OcclusionFactor { get; private set; }

	public override void Load()
	{
		AudioEffectsSystem.OnSoundUpdate += ApplyOcclusionToSounds;
	}

	private static void ApplyOcclusionToSounds(Span<AudioEffectsSystem.SoundData> sounds)
	{
		for (int i = 0; i < sounds.Length; i++) {
			ref var data = ref sounds[i];

			if (data.StartPosition is Vector2 startPosition) {
				float occlusion = CalculateSoundOcclusion(startPosition.ToTileCoordinates());

				data.Parameters.LowPassFiltering += occlusion;
			}
		}
	}

	private static float CalculateSoundOcclusion(Vector2Int position)
	{
		int occludingTiles = 0;

		const int MaxOccludingTiles = 15;

		foreach (var point in new GeometryUtils.BresenhamLine(CameraSystem.ScreenCenter.ToTileCoordinates(), position)) {
			if (!Main.tile.TryGet(point, out var tile)) {
				break;
			}

			bool solid = tile.HasTile && Main.tileSolid[tile.TileType];

			if (solid && ++occludingTiles >= MaxOccludingTiles) {
				break;
			}

			if (DebugSystem.EnableDebugRendering) {
				DebugSystem.DrawRectangle(new Rectangle(point.X, point.Y, 1, 1).ToWorldCoordinates(), solid ? Color.Orange : Color.GreenYellow, 1);
			}
		}

		return occludingTiles / (float)MaxOccludingTiles;
	}
}

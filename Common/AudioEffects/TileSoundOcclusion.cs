using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.Camera;
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
		float occlusionFactor = WallSoundOcclusion.OcclusionFactor;

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

		//TODO: Optimize, use on-stack enumerators instead of delegates.
		GeometryUtils.BresenhamLine(
			CameraSystem.ScreenCenter.ToTileCoordinates(),
			position,
			(Vector2Int point, ref bool stop) => {
				if (!Main.tile.TryGet(point, out var tile)) {
					stop = true;
					return;
				}

				if (tile.HasTile && Main.tileSolid[tile.TileType]) {
					occludingTiles++;

					if (occludingTiles >= MaxOccludingTiles) {
						stop = true;
					}
				}
			}
		);

		return occludingTiles / (float)MaxOccludingTiles;
	}
}

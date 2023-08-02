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

		Vector2Int areaCenter = player.Center.ToTileCoordinates();
		var halfSize = new Vector2Int(5, 5);
		Vector2Int size = halfSize * 2;
		Vector2Int start = areaCenter - halfSize;
		Vector2Int end = areaCenter + halfSize;

		const float RequiredWallRatio = 0.4f;

		int maxTiles = size.X * size.Y;
		int requiredWallTiles = (int)(maxTiles * RequiredWallRatio);
		int numWalls = 0;

		GeometryUtils.FloodFill(
			areaCenter - start,
			size,
			(Vector2Int p, out bool occupied, ref bool stop) => {
				int x = p.X + start.X;
				int y = p.Y + start.Y;
				Tile tile = Main.tile[x, y];

				occupied = tile.HasTile && Main.tileSolid[tile.TileType] && !Main.tileSolidTop[tile.TileType] && tile.BlockType == Terraria.ID.BlockType.Solid;

				if (!occupied && (tile.WallType > 0 || y >= Main.worldSurface)) {
					numWalls++;

					if (DebugSystem.EnableDebugRendering) {
						DebugSystem.DrawRectangle(new Rectangle(x * 16, y * 16, 16, 16), Color.Red, 1);
					}

					if (numWalls >= requiredWallTiles && !DebugSystem.EnableDebugRendering) {
						stop = true;
					}
				}
			}
		);

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

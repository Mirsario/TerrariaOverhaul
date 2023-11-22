using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.Camera;
using TerrariaOverhaul.Common.Tags;
using TerrariaOverhaul.Core.AudioEffects;
using TerrariaOverhaul.Core.Debugging;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.AudioEffects;

[Autoload(Side = ModSide.Client)]
public sealed class SurroundingsReverb : ModSystem
{
	private const int UpdateRate = 5;

	public static float MaxReverbIntensity => 0.725f;
	public static float MaxReverbTileRatio => 0.1f;

	public static Gradient<float> ReverbFactorToReverbIntensity => new(
		(0.0f, 0.00f),
		(0.2f, 0.00f),
		(0.3f, 0.30f),
		(1.0f, 1.00f)
	);

	public override void PostUpdateWorld()
	{
		if (Main.GameUpdateCount % UpdateRate != 0) {
			return;
		}

		if (Main.LocalPlayer is not Player { active: true } player) {
			return;
		}

		const int FloodFillRange = 22;
		const int FloodFillExtents = FloodFillRange / 2;

		Vector2Int areaCenter = CameraSystem.ScreenCenter.ToTileCoordinates();
		var areaRectangle = new Rectangle(areaCenter.X, areaCenter.Y, 0, 0).Extended(FloodFillExtents);

		int numReverbTiles = 0;
		int maxTiles = areaRectangle.Width * areaRectangle.Height;
		int maxReverbTiles = (int)(maxTiles * MaxReverbTileRatio) + 1;

		foreach (var p in new GeometryUtils.FloodFill(areaCenter, areaRectangle.ClampTileCoordinates())) {
			var (x, y) = p.Point;
			Tile tile = Main.tile[x, y];

			bool isPointFree = p.IsPointFree = !tile.HasTile || !Main.tileSolid[tile.TileType];

			if (isPointFree) {
				if (DebugSystem.EnableDebugRendering) {
					//DebugSystem.DrawRectangle(new Rectangle(x * 16, y * 16, 16, 16), Color.White, 1);
				}

				continue;
			}

			if (tile.TileType >= TileLoader.TileCount || !OverhaulTileTags.Reverb.Has(tile.TileType)) {
				continue;
			}

			if (DebugSystem.EnableDebugRendering) {
				DebugSystem.DrawRectangle(new Rectangle(x * 16, y * 16, 16, 16), Color.Red, 1);
			}

			numReverbTiles++;

			if (numReverbTiles >= maxReverbTiles) {
				break;
			}
		}

		float reverbTileFactor = numReverbTiles / (float)maxReverbTiles;
		float adjustedReverbTileFactor = ReverbFactorToReverbIntensity.GetValue(reverbTileFactor);
		float calculatedReverb = adjustedReverbTileFactor * MaxReverbIntensity;

		if (DebugSystem.EnableDebugRendering) {
			DebugSystem.DrawRectangle(areaRectangle.ToWorldCoordinates(), Color.Purple, 1);
		}

		//DebugSystem.Log($"{numReverbTiles} = {reverbTileFactor:0.00} = {adjustedReverbTileFactor:0.00} = {calculatedReverb:0.00}");

		if (calculatedReverb > 0f) {
			AudioEffectsSystem.AddAudioEffectModifier(
				60,
				$"{nameof(TerrariaOverhaul)}/{nameof(SurroundingsReverb)}",
				(float intensity, ref AudioEffectParameters soundParameters, ref AudioEffectParameters _) => {
					soundParameters.Reverb += calculatedReverb * intensity;
				}
			);
		}
	}
}

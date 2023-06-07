using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.Camera;
using TerrariaOverhaul.Common.Tags;
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

		Vector2Int areaCenter = CameraSystem.ScreenCenter.ToTileCoordinates();
		var halfSize = new Vector2Int(22, 22);
		Vector2Int size = halfSize * 2;
		Vector2Int start = areaCenter - halfSize;
		Vector2Int end = areaCenter + halfSize;

		int numReverbTiles = 0;
		int maxTiles = size.X * size.Y;
		int maxReverbTiles = (int)(maxTiles * MaxReverbTileRatio) + 1;

		GeometryUtils.FloodFill(
			areaCenter - start,
			size,
			(Vector2Int p, out bool occupied, ref bool stop) => {
				int x = p.X + start.X;
				int y = p.Y + start.Y;
				Tile tile = Main.tile[x, y];

				occupied = tile.HasTile && Main.tileSolid[tile.TileType];

				if (!occupied) {
					/*if(DebugSystem.EnableDebugRendering) {
						DebugSystem.DrawRectangle(new Rectangle(x * 16, y * 16, 16, 16), Color.White, 1);
					}*/

					return;
				}

				if (tile.TileType >= TileLoader.TileCount || !OverhaulTileTags.Reverb.Has(tile.TileType)) {
					return;
				}

				if (DebugSystem.EnableDebugRendering) {
					DebugSystem.DrawRectangle(new Rectangle(x * 16, y * 16, 16, 16), Color.Red, 1);
				}

				numReverbTiles++;

				if (numReverbTiles >= maxReverbTiles) {
					stop = true;
				}
			}
		);

		float reverbTileFactor = numReverbTiles / (float)maxReverbTiles;
		float adjustedReverbTileFactor = ReverbFactorToReverbIntensity.GetValue(reverbTileFactor);
		float calculatedReverb = adjustedReverbTileFactor * MaxReverbIntensity;

		if (DebugSystem.EnableDebugRendering) {
			DebugSystem.DrawRectangle(new Rectangle(start.X * 16, start.Y * 16, halfSize.X * 32, halfSize.Y * 32), Color.Purple, 4);
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

using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.Systems.AudioEffects;
using TerrariaOverhaul.Common.Systems.Camera;
using TerrariaOverhaul.Common.Tags;
using TerrariaOverhaul.Core.Systems.Debugging;
using TerrariaOverhaul.Utilities.DataStructures;
using TerrariaOverhaul.Utilities.Extensions;

namespace TerrariaOverhaul.Common.ModEntities.Players
{
	[Autoload(Side = ModSide.Client)]
	public sealed class PlayerSurroundingsReverb : ModPlayer
	{
		public static float MaxReverbIntensity => 0.725f;
		public static float MaxReverbTileRatio => 0.08f;

		public static Gradient<float> ReverbFactorToReverbIntensity => new(
			(0.0f, 0.00f),
			(0.1f, 0.01f),
			(1.0f, 1.00f)
		);

		public override void PostUpdate()
		{
			if(!Player.IsLocal() || Main.GameUpdateCount % 5 != 0) {
				return;
			}

			Vector2Int areaCenter = CameraSystem.ScreenCenter.ToTileCoordinates();
			Vector2Int halfSize = new Vector2Int(20, 20);
			Vector2Int size = halfSize * 2;
			Vector2Int start = areaCenter - halfSize;
			Vector2Int end = areaCenter + halfSize;

			int numReverbTiles = 0;
			int maxTiles = size.X * size.Y;
			int maxReverbTiles = (int)(maxTiles * MaxReverbTileRatio) + 1;

			static bool CheckForAir(int x, int y)
			{
				if(!Main.tile.TryGet(x, y, out var tile)) {
					return false;
				}

				return !tile.IsActive || !Main.tileSolid[tile.type];
			}

			for(int y = start.Y; y >= start.Y && y <= end.Y; y++) {
				for(int x = start.X; x >= start.X && x <= end.X; x++) {
					if(!Main.tile.TryGet(x, y, out var tile)) {
						continue;
					}

					if(tile.IsActive && tile.type < TileLoader.TileCount && OverhaulTileTags.Reverb.Has(tile.type) && (CheckForAir(x - 1, y) || CheckForAir(x + 1, y) || CheckForAir(x, y - 1) || CheckForAir(x, y + 1))) {
						numReverbTiles++;

						if(DebugSystem.EnableDebugRendering) {
							DebugSystem.DrawRectangle(new Rectangle(x * 16, y * 16, 16, 16), Color.SteelBlue, 1);
						}

						if(numReverbTiles >= maxReverbTiles) {
							y = int.MaxValue;
							break;
						}
					}
				}
			}

			float reverbTileFactor = numReverbTiles / (float)maxReverbTiles;
			float adjustedReverbTileFactor = ReverbFactorToReverbIntensity.GetValue(reverbTileFactor);
			float calculatedReverb = adjustedReverbTileFactor * MaxReverbIntensity;

			if(DebugSystem.EnableDebugRendering) {
				DebugSystem.DrawRectangle(new Rectangle(start.X * 16, start.Y * 16, halfSize.X * 32, halfSize.Y * 32), Color.Purple, 4);
			}

			//DebugSystem.Log($"{numReverbTiles} = {reverbTileFactor:0.00} = {adjustedReverbTileFactor:0.00} = {calculatedReverb:0.00}");

			if(calculatedReverb > 0f) {
				AudioEffectsSystem.AddAudioEffectModifier(
					60,
					$"{nameof(TerrariaOverhaul)}/{nameof(PlayerSurroundingsReverb)}",
					(float intensity, ref float reverbIntensity, ref float _) => reverbIntensity += calculatedReverb * intensity
				);
			}
		}
	}
}

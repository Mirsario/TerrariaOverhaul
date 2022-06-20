using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using TerrariaOverhaul.Common.AudioEffects;
using TerrariaOverhaul.Core.Time;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.Ambience.Sounds
{
	public sealed class ForestBirdsAmbienceTrack : AmbienceTrack
	{
		public override void Initialize()
		{
			Sound = new SoundStyle($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Ambience/Forest/ForestBirds", SoundType.Ambient) {
				Volume = 0.38f,
				IsLooped = true,
			};

			AudioEffectsSystem.EnableSoundStyleWallOcclusion(Sound.Value);
		}

		public override float GetTargetVolume(Player localPlayer)
		{
			// Only in purity
			if (!localPlayer.ZonePurity) {
				return 0f;
			}

			float result = 1f;

			// During day
			result *= TimeSystem.DayGradient.GetValue(TimeSystem.RealTime);
			// On the surface
			result *= WorldLocationUtils.SurfaceGradient.GetValue(localPlayer.Center.ToTileCoordinates().Y);
			// When it's not raining too much
			result *= MathHelper.Clamp(1f - Main.maxRaining * 2f, 0f, 1f);

			return result;
		}
	}
}

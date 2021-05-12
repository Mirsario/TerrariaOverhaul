using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.Systems.AudioEffects;
using TerrariaOverhaul.Common.Systems.Time;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.Systems.Ambience.Sounds
{
	public sealed class ForestBirdsAmbienceTrack : AmbienceTrack
	{
		public override void Initialize()
		{
			Sound = new ModSoundStyle(nameof(TerrariaOverhaul), "Assets/Sounds/Ambience/Forest/ForestBirds", volume: 0.5f, type: SoundType.Ambient);

			AudioEffectsSystem.EnableSoundStyleWallOcclusion(Sound);
		}
		public override float GetTargetVolume(Player localPlayer)
		{
			//Only in purity
			if(!localPlayer.ZonePurity) {
				return 0f;
			}

			float result = 1f;
			
			//During day
			result *= TimeSystem.DayGradient.GetValue(TimeSystem.RealTime);
			//On the surface
			result *= WorldLocationUtils.SurfaceGradient.GetValue(localPlayer.Center.ToTileCoordinates().Y);
			//When it's not raining too much
			result *= MathHelper.Clamp(1f - Main.maxRaining * 2f, 0f, 1f);

			return result;
		}
	}
}

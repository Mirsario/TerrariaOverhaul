using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.Systems.Time;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.Systems.Ambience.Sounds
{
	public sealed class ForestBirdsAmbienceTrack : AmbienceTrack
	{
		public override void Initialize()
		{
			Sound = new ModSoundStyle(nameof(TerrariaOverhaul), "Assets/Sounds/Atmosphere/Forest/ForestBirds", volume: 0.5f, type: SoundType.Ambient);
		}
		public override float GetTargetVolume(Player localPlayer)
		{
			if(!localPlayer.ZonePurity) {
				return 0f;
			}

			float result = 1f;
			
			result *= TimeSystem.DayGradient.GetValue(TimeSystem.RealTime);
			result *= WorldLocationUtils.SurfaceGradient.GetValue(localPlayer.Center.ToTileCoordinates().Y);

			return result;
		}
	}
}

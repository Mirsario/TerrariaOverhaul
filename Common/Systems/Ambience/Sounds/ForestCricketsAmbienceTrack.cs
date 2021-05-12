using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.Systems.AudioEffects;
using TerrariaOverhaul.Common.Systems.Time;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.Systems.Ambience.Sounds
{
	public sealed class ForestCricketsAmbienceTrack : AmbienceTrack
	{
		public override void Initialize()
		{
			Sound = new ModSoundStyle(nameof(TerrariaOverhaul), "Assets/Sounds/Atmosphere/Forest/ForestCrickets", type: SoundType.Ambient);

			AudioEffectsSystem.EnableSoundStyleWallOcclusion(Sound);
		}
		public override float GetTargetVolume(Player localPlayer)
		{
			if(!localPlayer.ZonePurity) {
				return 0f;
			}

			float result = 1f;

			result *= TimeSystem.NightGradient.GetValue(TimeSystem.RealTime);
			result *= WorldLocationUtils.SurfaceGradient.GetValue(localPlayer.Center.ToTileCoordinates().Y);

			return result;
		}
	}
}

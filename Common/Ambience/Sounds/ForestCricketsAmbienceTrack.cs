using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.AudioEffects;
using TerrariaOverhaul.Core.Time;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.Ambience.Sounds
{
	public sealed class ForestCricketsAmbienceTrack : AmbienceTrack
	{
		public override void Initialize()
		{
			Sound = new ModSoundStyle($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Ambience/Forest/ForestCrickets", type: SoundType.Ambient);

			AudioEffectsSystem.EnableSoundStyleWallOcclusion(Sound);
		}

		public override float GetTargetVolume(Player localPlayer)
		{
			if (!localPlayer.ZonePurity) {
				return 0f;
			}

			float result = 1f;

			// During night
			result *= TimeSystem.NightGradient.GetValue(TimeSystem.RealTime);
			// On the surface
			result *= WorldLocationUtils.SurfaceGradient.GetValue(localPlayer.Center.ToTileCoordinates().Y);

			return result;
		}
	}
}

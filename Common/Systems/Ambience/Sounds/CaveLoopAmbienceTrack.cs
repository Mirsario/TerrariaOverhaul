using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.Systems.Ambience.Sounds
{
	public sealed class CaveLoopAmbienceTrack : AmbienceTrack
	{
		public override void Initialize()
		{
			Sound = new ModSoundStyle($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Ambience/Underground/CaveLoop", volume: 0.2f, type: SoundType.Ambient);
		}

		public override float GetTargetVolume(Player localPlayer)
		{
			float result = 1f;

			result *= WorldLocationUtils.UnderSurfaceGradient.GetValue(localPlayer.Center.ToTileCoordinates().Y);

			return result;
		}
	}
}

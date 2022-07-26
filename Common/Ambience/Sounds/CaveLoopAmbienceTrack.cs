using Terraria;
using Terraria.Audio;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.Ambience.Sounds;

public sealed class CaveLoopAmbienceTrack : AmbienceTrack
{
	public override void Initialize()
	{
		Sound = new($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Ambience/Underground/CaveLoop", SoundType.Ambient) {
			Volume = 0.2f,
			IsLooped = true,
		};
	}

	public override float GetTargetVolume(Player localPlayer)
	{
		float result = 1f;

		result *= WorldLocationUtils.UnderSurfaceGradient.GetValue(localPlayer.Center.ToTileCoordinates().Y);

		return result;
	}
}

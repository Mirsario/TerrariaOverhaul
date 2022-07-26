using Terraria;
using Terraria.Audio;

namespace TerrariaOverhaul.Common.Ambience.Sounds;

public sealed class EvilDroneLoopAmbienceTrack : AmbienceTrack
{
	public override void Initialize()
	{
		Sound = new($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Ambience/Evil/EvilDroneLoop", SoundType.Ambient) {
			Volume = 0.5f,
			IsLooped = true,
		};
	}

	public override float GetTargetVolume(Player localPlayer)
	{
		if (!localPlayer.ZoneCorrupt && !localPlayer.ZoneCrimson && !localPlayer.ZoneMeteor && !localPlayer.ZoneDungeon) {
			return 0f;
		}

		return 1f;
	}
}

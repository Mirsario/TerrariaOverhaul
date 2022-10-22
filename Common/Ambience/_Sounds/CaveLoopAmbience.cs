using Terraria.Audio;

namespace TerrariaOverhaul.Common.Ambience;

public sealed class CaveLoopAmbience : AmbienceTrack
{
	public override void Initialize()
	{
		Sound = new($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Ambience/Underground/CaveLoop", SoundType.Ambient) {
			Volume = 0.2f,
			IsLooped = true,
		};
		Signals = new SignalContainer[] {
			new("UnderSurfaceAltitude"),
		};
	}
}

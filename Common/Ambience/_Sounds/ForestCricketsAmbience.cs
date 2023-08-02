using Terraria.Audio;
using TerrariaOverhaul.Common.AudioEffects;

namespace TerrariaOverhaul.Common.Ambience;

public sealed class ForestCricketsAmbience : AmbienceTrack
{
	public override void Initialize()
	{
		Sound = new($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Ambience/Forest/ForestCrickets", SoundType.Ambient) {
			Volume = 0.13f,
			IsLooped = true,
		};
		Signals = new SignalContainer[] {
			// Biomes
			new(SignalFlags.Inverse, "Corruption", "Crimson"),
			// Etc.
			new("NightTime"),
			new("SurfaceAltitude"),
			new("NotRainWeather"),
		};

		WallSoundOcclusion.SetEnabledForSoundStyle(Sound, true);
	}
}

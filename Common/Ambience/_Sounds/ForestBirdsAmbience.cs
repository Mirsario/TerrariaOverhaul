using Terraria.Audio;
using TerrariaOverhaul.Common.AudioEffects;

namespace TerrariaOverhaul.Common.Ambience;

public sealed class ForestBirdsAmbience : AmbienceTrack
{
	public override void Initialize()
	{
		Sound = new SoundStyle($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Ambience/Forest/ForestBirds", SoundType.Ambient) {
			Volume = 0.45f,
			IsLooped = true,
		};
		Signals = new SignalContainer[] {
			// Biomes
			new(SignalFlags.Inverse, "Corruption", "Crimson", "Jungle", "Desert", "Tundra"),
			// Etc.
			new("DayTime"),
			new("SurfaceAltitude"),
			new("NotRainWeather"),
			new("TreesAround"),
		};

		WallSoundOcclusion.SetEnabledForSoundStyle(Sound, true);
	}
}

using Terraria.Audio;
using TerrariaOverhaul.Common.AudioEffects;
using TerrariaOverhaul.Core.Tags;

namespace TerrariaOverhaul.Common.Ambience;

public sealed class ForestBirdsAmbience : AmbienceTrack
{
	public override void Initialize()
	{
		Sound = new SoundStyle($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Ambience/Forest/ForestBirds", SoundType.Ambient) {
			Volume = 0.38f,
			IsLooped = true,
		};
		Conditions = new TagCondition[] {
			new(TagCondition.ConditionType.All, "Purity"),
		};
		VolumeMultipliers = new VolumeMultiplier.Function[] {
			VolumeMultiplier.DayTime,
			VolumeMultiplier.SurfaceAltitude,
			VolumeMultiplier.NotRainWeather,
		};

		AudioEffectsSystem.EnableSoundStyleWallOcclusion(Sound);
	}
}

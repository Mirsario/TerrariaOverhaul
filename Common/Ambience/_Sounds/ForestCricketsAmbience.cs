using Terraria.Audio;
using TerrariaOverhaul.Common.AudioEffects;
using TerrariaOverhaul.Core.Tags;

namespace TerrariaOverhaul.Common.Ambience;

public sealed class ForestCricketsAmbience : AmbienceTrack
{
	public override void Initialize()
	{
		Sound = new($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Ambience/Forest/ForestCrickets", SoundType.Ambient) {
			Volume = 0.13f,
			IsLooped = true,
		};

		Conditions = new TagCondition[] {
			new(TagCondition.ConditionType.All, "Purity"),
		};
		VolumeMultipliers = new VolumeMultiplier.Function[] {
			VolumeMultiplier.NightTime,
			VolumeMultiplier.SurfaceAltitude,
		};

		AudioEffectsSystem.EnableSoundStyleWallOcclusion(Sound);
	}
}

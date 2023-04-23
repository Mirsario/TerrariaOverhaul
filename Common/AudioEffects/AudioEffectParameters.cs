using Microsoft.Xna.Framework;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.AudioEffects;

public struct AudioEffectParameters
{
	private float volumeScale = 1f;
	private float reverbIntensity = 0f;
	private float lowPassFilteringIntensity = 0f;

	public float Volume {
		get => volumeScale;
		set => volumeScale = MathHelper.Clamp(value, 0f, 1f);
	}
	public float Reverb {
		get => reverbIntensity;
		set => reverbIntensity = MathHelper.Clamp(value, 0f, 1f);
	}
	public float LowPassFiltering {
		get => lowPassFilteringIntensity;
		set => lowPassFilteringIntensity = MathHelper.Clamp(value, 0f, 1f);
	}

	public AudioEffectParameters() { }

	static AudioEffectParameters()
	{
		Gradient<AudioEffectParameters>.Lerp = Lerp;
	}

	public static AudioEffectParameters Lerp(AudioEffectParameters a, AudioEffectParameters b, float step)
		=> Lerp(in a, in b, step);

	public static AudioEffectParameters Lerp(in AudioEffectParameters a, in AudioEffectParameters b, float step)
	{
		AudioEffectParameters result = default;

		result.Volume = MathHelper.Lerp(a.Volume, a.Volume, step);
		result.Reverb = MathHelper.Lerp(a.Reverb, a.Reverb, step);
		result.LowPassFiltering = MathHelper.Lerp(a.LowPassFiltering, a.LowPassFiltering, step);

		return result;
	}
}

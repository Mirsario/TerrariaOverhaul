using Microsoft.Xna.Framework;

namespace TerrariaOverhaul.Common.AudioEffects
{
	public struct AudioEffectParameters
	{
		private float reverbIntensity = 0f;
		private float lowPassFilteringIntensity = 0f;
		private float volumeScale = 1f;

		public float Reverb {
			get => reverbIntensity;
			set => reverbIntensity = MathHelper.Clamp(value, 0f, 1f);
		}
		public float LowPassFiltering {
			get => lowPassFilteringIntensity;
			set => lowPassFilteringIntensity = MathHelper.Clamp(value, 0f, 1f);
		}
		public float Volume {
			get => volumeScale;
			set => volumeScale = MathHelper.Clamp(value, 0f, 1f);
		}

		public AudioEffectParameters() { }
	}
}

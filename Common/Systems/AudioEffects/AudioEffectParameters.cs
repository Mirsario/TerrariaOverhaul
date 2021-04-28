using Microsoft.Xna.Framework;

namespace TerrariaOverhaul.Common.Systems.AudioEffects
{
	public struct AudioEffectParameters
	{
		public static readonly AudioEffectParameters Default = new() {
			Volume = 1f
		};

		private float reverbIntensity;
		private float lowPassFilteringIntensity;
		private float volumeScale;

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
	}
}

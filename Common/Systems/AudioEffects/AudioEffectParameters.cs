using Microsoft.Xna.Framework;

namespace TerrariaOverhaul.Common.Systems.AudioEffects
{
	public struct AudioEffectParameters
	{
		private float reverbIntensity;
		private float lowPassFilteringIntensity;

		public float Reverb {
			get => reverbIntensity;
			set => reverbIntensity = MathHelper.Clamp(value, 0f, 1f);
		}
		public float LowPassFiltering {
			get => lowPassFilteringIntensity;
			set => lowPassFilteringIntensity = MathHelper.Clamp(value, 0f, 1f);
		}
	}
}

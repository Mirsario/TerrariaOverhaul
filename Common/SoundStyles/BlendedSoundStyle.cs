/*using System;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Terraria.Audio;

namespace TerrariaOverhaul.Common.SoundStyles
{
	public struct BlendedSoundStyle : ISoundStyle
	{
		public ISoundStyle StyleA { get; private set; }
		public ISoundStyle StyleB { get; private set; }
		public float Factor { get; set; }

		public float Volume => (StyleA.Volume + StyleB.Volume) * 0.5f;
		public SoundType Type => StyleA.Type;

		public BlendedSoundStyle(ISoundStyle styleA, ISoundStyle styleB, float factor, float volume = 1f)
		{
			StyleA = styleA;
			StyleB = styleB;
			Factor = factor;
		}

		//TODO: This is a very temporary implementation. Should improve with improvements to tModLoader:1.4_soundfix
		public override SoundEffectInstance Play(Vector2? position)
		{
			var method = typeof(object).GetMethod("MemberwiseClone", BindingFlags.Instance | BindingFlags.NonPublic);

			var cloneA = StyleA;
			var cloneB = StyleB;

			cloneA.Volume *= Volume * (1f - Factor);
			cloneB.Volume *= Volume * Factor;

			var instanceA = cloneA.Play(position);
			var instanceB = cloneB.Play(position);

			return Factor <= 0.5f ? instanceA : instanceB;
		}

		public SoundEffect GetRandomSound() => StyleA.GetRandomSound();
		public float GetRandomPitch() => StyleA.GetRandomPitch();
	}
}*/

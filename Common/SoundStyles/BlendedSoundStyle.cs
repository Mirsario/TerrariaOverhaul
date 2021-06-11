using System;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Terraria.Audio;

namespace TerrariaOverhaul.Common.SoundStyles
{
	public class BlendedSoundStyle : SoundStyle
	{
		public override bool IsTrackable => true;

		public SoundStyle StyleA { get; private set; }
		public SoundStyle StyleB { get; private set; }
		public float Factor { get; set; }

		public BlendedSoundStyle(SoundStyle styleA, SoundStyle styleB, float factor, float volume = 1f) : base(volume, 0f, 0f, SoundType.Sound)
		{
			StyleA = styleA;
			StyleB = styleB;
			Factor = factor;
		}

		//TODO: This is a very temporary implementation. Should improve with improvements to tModLoader:1.4_soundfix
		public override SoundEffectInstance Play(Vector2? position)
		{
			var method = typeof(object).GetMethod("MemberwiseClone", BindingFlags.Instance | BindingFlags.NonPublic);

			var cloneA = (SoundStyle)method.Invoke(StyleA, null);
			var cloneB = (SoundStyle)method.Invoke(StyleB, null);

			cloneA.Volume *= Volume * (1f - Factor);
			cloneB.Volume *= Volume * Factor;

			var instanceA = cloneA.Play(position);
			var instanceB = cloneB.Play(position);

			//StyleA.Volume = oldVolumeA;
			//StyleB.Volume = oldVolumeB;

			return Factor <= 0.5f ? instanceA : instanceB;
		}
		
		public override SoundEffect GetRandomSound() => throw new NotImplementedException();
	}
}

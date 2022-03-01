using Microsoft.Xna.Framework;
using ReLogic.Utilities;
using Terraria.Audio;

namespace TerrariaOverhaul.Utilities
{
	public static class SoundUtils
	{
		public static void UpdateLoopingSound(ref SlotId slot, ISoundStyle style, float volume, Vector2 position)
		{
			var sound = slot.IsValid ? SoundEngine.GetActiveSound(slot) : null;

			if (volume > 0f) {
				//float styleVolume = style.Volume;

				try {
					if (sound == null) {
						//style.Volume = 0f;
						slot = SoundEngine.PlayTrackedSound(style, position);
						sound = SoundEngine.GetActiveSound(slot);

						if (sound == null) {
							return;
						}
					}

					sound.Position = position;
					sound.Volume = volume;
				}
				finally {
					//style.Volume = styleVolume;
				}
			} else if (sound != null) {
				sound.Stop();

				slot = SlotId.Invalid;
			}
		}
	}
}

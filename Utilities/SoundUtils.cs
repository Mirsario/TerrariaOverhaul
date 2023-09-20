using Microsoft.Xna.Framework;
using ReLogic.Utilities;
using Terraria.Audio;

namespace TerrariaOverhaul.Utilities;

public static class SoundUtils
{
	public static void UpdateLoopingSound(ref SlotId slot, SoundStyle style, float volume, Vector2? position)
	{
		SoundEngine.TryGetActiveSound(slot, out var sound);

		if (volume > 0f) {
			if (sound == null) {
				slot = SoundEngine.PlaySound(style, position);
				return;
			}

			sound.Position = position;
			sound.Volume = volume;
		} else if (sound != null) {
			sound.Stop();

			slot = SlotId.Invalid;
		}
	}
}

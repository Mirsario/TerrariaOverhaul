// Copyright (c) 2020-2026 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using TerrariaOverhaul.Core.Configuration;

namespace TerrariaOverhaul.Common.GrapplingHooks;

internal static class GrapplingHookSounds
{
	public static readonly ConfigEntry<bool> EnableGrapplingHookSounds = new(ConfigSide.ClientOnly, true, "Movement");

	// TODO: Replace these vanilla placeholder sounds with custom assets.
	// Place .ogg files in Assets/Sounds/GrapplingHooks/ and update these to SoundStyle constants:
	// public static readonly SoundStyle ThrowSound = new($"{nameof(TerrariaOverhaul)}/Assets/Sounds/GrapplingHooks/Throw", 3) { Volume = 0.5f, PitchVariance = 0.1f };

	public static void PlayThrowSound(Vector2 position)
	{
		if (!EnableGrapplingHookSounds) {
			return;
		}
		SoundEngine.PlaySound(SoundID.Item1, position);
	}

	public static void PlayLatchSound(Vector2 position)
	{
		if (!EnableGrapplingHookSounds) {
			return;
		}
		SoundEngine.PlaySound(SoundID.Dig, position);
	}

	public static void PlayAdjustSound(Vector2 position)
	{
		if (!EnableGrapplingHookSounds) {
			return;
		}
		SoundEngine.PlaySound(SoundID.Item15, position);
	}

	public static void PlaySwingSound(Vector2 position, float volumeScale = 1f)
	{
		if (!EnableGrapplingHookSounds) {
			return;
		}
		// Placeholder: low-volume mechanical sound scaled by swing speed
		SoundEngine.PlaySound(SoundID.Item15, position);
	}
}

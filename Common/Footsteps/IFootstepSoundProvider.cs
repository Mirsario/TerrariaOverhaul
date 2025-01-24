// Copyright (c) 2020-2025 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using Terraria.Audio;

namespace TerrariaOverhaul.Common.Footsteps;

public interface IFootstepSoundProvider
{
	SoundStyle? FootstepSound { get; }

	SoundStyle? JumpFootstepSound => FootstepSound;
	SoundStyle? LandFootstepSound => FootstepSound;
}

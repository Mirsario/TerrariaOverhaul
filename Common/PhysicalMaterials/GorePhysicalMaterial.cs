// Copyright (c) 2020-2025 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using Terraria.Audio;
using TerrariaOverhaul.Common.Footsteps;
using TerrariaOverhaul.Core.PhysicalMaterials;

namespace TerrariaOverhaul.Common.PhysicalMaterials;

public sealed class GorePhysicalMaterial : PhysicalMaterial, IFootstepSoundProvider
{
	public SoundStyle? FootstepSound { get; } = new($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Gore/GoreSmallSplatter", 2) {
		Volume = 0.4f,
		PitchVariance = 0.1f,
	};
}

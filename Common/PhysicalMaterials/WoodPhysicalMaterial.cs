// Copyright (c) 2020-2025 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using Terraria.Audio;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.Footsteps;
using TerrariaOverhaul.Core.PhysicalMaterials;
using TerrariaOverhaul.Core.Tags;

namespace TerrariaOverhaul.Common.PhysicalMaterials;

public sealed class WoodPhysicalMaterial : PhysicalMaterial, IContentSetAssociated, IFootstepSoundProvider
{
	public ContentSet ContentSet { get; } = "WoodFootsteps";

	// Footsteps
	public SoundStyle? FootstepSound { get; } = new($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Footsteps/Wood/Step", 11) {
		Volume = 0.5f,
		PitchVariance = 0.1f,
	};

	public SoundStyle? JumpFootstepSound => ModContent.GetInstance<StonePhysicalMaterial>().JumpFootstepSound;
}

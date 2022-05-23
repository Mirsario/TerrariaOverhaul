using Terraria.Audio;
using Terraria.ModLoader;
using Terraria.ModLoader.Tags;
using TerrariaOverhaul.Common.Footsteps;
using TerrariaOverhaul.Common.Tags;
using TerrariaOverhaul.Core.PhysicalMaterials;

namespace TerrariaOverhaul.Common.PhysicalMaterials
{
	public sealed class GrassPhysicalMaterial : PhysicalMaterial, ITileTagAssociated, IFootstepSoundProvider
	{
		public TagData TileTag { get; } = OverhaulTileTags.Grass;

		// Footsteps
		public SoundStyle? FootstepSound { get; } = new($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Footsteps/Grass/Step", 8) {
			Volume = 0.5f,
		};

		public SoundStyle? JumpFootstepSound => ModContent.GetInstance<DirtPhysicalMaterial>().JumpFootstepSound;
	}
}

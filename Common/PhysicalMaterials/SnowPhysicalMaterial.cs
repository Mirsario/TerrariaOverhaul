using Terraria.Audio;
using Terraria.ModLoader.Tags;
using TerrariaOverhaul.Common.Footsteps;
using TerrariaOverhaul.Common.Tags;
using TerrariaOverhaul.Core.PhysicalMaterials;

namespace TerrariaOverhaul.Common.PhysicalMaterials
{
	public sealed class SnowPhysicalMaterial : PhysicalMaterial, ITileTagAssociated, IFootstepSoundProvider
	{
		public TagData TileTag { get; } = OverhaulTileTags.Snow;

		public SoundStyle? FootstepSound { get; } = new($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Footsteps/Snow/Step", 11) {
			Volume = 0.5f,
		};
	}
}

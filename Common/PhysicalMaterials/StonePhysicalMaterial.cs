using Terraria.Audio;
using Terraria.ModLoader.Tags;
using TerrariaOverhaul.Common.Footsteps;
using TerrariaOverhaul.Common.Tags;
using TerrariaOverhaul.Core.PhysicalMaterials;

namespace TerrariaOverhaul.Common.PhysicalMaterials
{
	public sealed class StonePhysicalMaterial : PhysicalMaterial, ITileTagAssociated, IFootstepSoundProvider
	{
		public TagData TileTag { get; } = OverhaulTileTags.Stone;
		// Footsteps
		public SoundStyle? FootstepSound { get; } = new($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Footsteps/Stone/Step", 8) {
			Volume = 0.5f,
		};
		public SoundStyle? JumpFootstepSound { get; } = new($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Footsteps/Stone/Jump", 3) {
			Volume = 0.5f,
		};
	}
}

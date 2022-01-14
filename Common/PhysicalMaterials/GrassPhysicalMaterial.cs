using Terraria.Audio;
using Terraria.ModLoader;
using Terraria.ModLoader.Tags;
using TerrariaOverhaul.Common.Systems.Footsteps;
using TerrariaOverhaul.Common.Tags;
using TerrariaOverhaul.Core.Systems.PhysicalMaterials;

namespace TerrariaOverhaul.Common.PhysicalMaterials
{
	public sealed class GrassPhysicalMaterial : PhysicalMaterial, ITileTagAssociated, IFootstepSoundProvider
	{
		public TagData TileTag { get; } = OverhaulTileTags.Grass;

		// Footsteps
		public ISoundStyle FootstepSound { get; } = new ModSoundStyle($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Footsteps/Grass/Step", 8, volume: 0.5f);

		public ISoundStyle JumpFootstepSound => ModContent.GetInstance<DirtPhysicalMaterial>().JumpFootstepSound;
	}
}

using Terraria.Audio;
using Terraria.ModLoader;
using Terraria.ModLoader.Tags;
using TerrariaOverhaul.Common.Systems.Footsteps;
using TerrariaOverhaul.Common.Tags;
using TerrariaOverhaul.Core.Systems.PhysicalMaterials;

namespace TerrariaOverhaul.Common.PhysicalMaterials
{
	public sealed class StonePhysicalMaterial : PhysicalMaterial, ITileTagAssociated, IFootstepSoundProvider
	{
		public TagData TileTag { get; } = OverhaulTileTags.Stone;
		// Footsteps
		public ISoundStyle FootstepSound { get; } = new ModSoundStyle($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Footsteps/Stone/Step", 8, volume: 0.5f);
		public ISoundStyle JumpFootstepSound { get; } = new ModSoundStyle($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Footsteps/Stone/Jump", 3, volume: 0.5f);
	}
}

using Terraria.Audio;
using Terraria.ModLoader;
using Terraria.ModLoader.Tags;
using TerrariaOverhaul.Common.Systems.Footsteps;
using TerrariaOverhaul.Common.Tags;
using TerrariaOverhaul.Core.Systems.PhysicalMaterials;

namespace TerrariaOverhaul.Common.PhysicalMaterials
{
	public sealed class DirtPhysicalMaterial : PhysicalMaterial, ITileTagAssociated, IFootstepSoundProvider
	{
		public TagData TileTag { get; } = OverhaulTileTags.Dirt;
		//Footsteps
		public SoundStyle FootstepSound { get; } = new ModSoundStyle(nameof(TerrariaOverhaul), "Assets/Sounds/Footsteps/Dirt/Step", 8, volume: 0.5f);
		public SoundStyle JumpFootstepSound { get; } = new ModSoundStyle(nameof(TerrariaOverhaul), "Assets/Sounds/Footsteps/Dirt/Jump", 3, volume: 0.5f);
	}
}

using Terraria.Audio;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.Systems.Footsteps;
using TerrariaOverhaul.Core.Systems.PhysicalMaterials;

namespace TerrariaOverhaul.Common.PhysicalMaterials
{
	public sealed class GorePhysicalMaterial : PhysicalMaterial, IFootstepSoundProvider
	{
		public ISoundStyle FootstepSound { get; } = new ModSoundStyle($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Gore/GoreSmallSplatter", 2, volume: 0.4f);
	}
}

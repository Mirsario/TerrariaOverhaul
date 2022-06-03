using Terraria.Audio;
using Terraria.ModLoader;

namespace TerrariaOverhaul.Common.Fires
{
	public class FireSystem : ModSystem
	{
		public static readonly SoundStyle ExtinguishSound = new($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Fire/Extinguish") {
			PitchVariance = 0.1f
		};
	}
}

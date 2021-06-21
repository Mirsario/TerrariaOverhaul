using Terraria.Audio;
using Terraria.ModLoader;

namespace TerrariaOverhaul.Common.Systems.Fires
{
	public class FireSystem : ModSystem
	{
		public static readonly SoundStyle ExtinguishSound = new ModSoundStyle($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Fire/Extinguish", 0, pitchVariance: 0.1f);
	}
}

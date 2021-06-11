using System.Collections.Generic;
using Terraria.ModLoader;
using TerrariaOverhaul.Core.Systems.Configuration;

namespace TerrariaOverhaul.Common.Systems.Ambience
{
	[Autoload(Side = ModSide.Client)]
	public sealed class AmbienceSystem : ModSystem
	{
		public static readonly ConfigEntry<bool> EnableAmbientSounds = new(ConfigSide.ClientOnly, "Ambience", nameof(EnableAmbientSounds), () => true);

		private static readonly List<AmbienceTrack> Tracks = new List<AmbienceTrack>();

		public override void PostUpdateWorld() => UpdateAmbienceTracks();

		private void UpdateAmbienceTracks()
		{
			for(int i = 0; i < Tracks.Count; i++) {
				Tracks[i].Update();
			}
		}

		internal static void RegisterAmbienceTrack(AmbienceTrack track)
		{
			Tracks.Add(track);
		}
	}
}

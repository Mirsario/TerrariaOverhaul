using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;

namespace TerrariaOverhaul.Common.Systems.Ambience.Sounds
{
	public sealed class EvilDroneLoopAmbienceTrack : AmbienceTrack
	{
		public override void Initialize()
		{
			Sound = new ModSoundStyle(nameof(TerrariaOverhaul), "Assets/Sounds/Ambience/Evil/EvilDroneLoop", volume: 0.5f, type: SoundType.Ambient);
		}
		
		public override float GetTargetVolume(Player localPlayer)
		{
			if(!localPlayer.ZoneCorrupt && !localPlayer.ZoneCrimson && !localPlayer.ZoneMeteor && !localPlayer.ZoneDungeon) {
				return 0f;
			}

			return 1f;
		}
	}
}

using System;
using Terraria;
using Terraria.Audio;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.Ambience;

public sealed class CaveLoopAmbience : AmbienceTrack
{
	public override void Initialize()
	{
		Sound = new($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Ambience/Underground/CaveLoop", SoundType.Ambient) {
			Volume = 0.2f,
			IsLooped = true,
		};

		VolumeMultipliers = new VolumeMultiplier.Function[] {
			VolumeMultiplier.UnderSurfaceAltitude,
		};
	}
}

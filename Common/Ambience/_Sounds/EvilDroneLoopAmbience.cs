using Terraria.Audio;

namespace TerrariaOverhaul.Common.Ambience;

public sealed class EvilDroneLoopAmbience : AmbienceTrack
{
	public override void Initialize()
	{
		Sound = new($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Ambience/Evil/EvilDroneLoop", SoundType.Ambient) {
			Volume = 0.6f,
			IsLooped = true,
		};
		Signals = new SignalContainer[] {
			new(SignalFlags.Max, "Corruption", "Crimson", "Meteor", "Dungeon"),
		};
	}
}

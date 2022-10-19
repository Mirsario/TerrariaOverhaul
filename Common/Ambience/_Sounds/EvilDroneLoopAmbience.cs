using Terraria;
using Terraria.Audio;
using TerrariaOverhaul.Core.Tags;

namespace TerrariaOverhaul.Common.Ambience;

public sealed class EvilDroneLoopAmbience : AmbienceTrack
{
	public override void Initialize()
	{
		Sound = new($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Ambience/Evil/EvilDroneLoop", SoundType.Ambient) {
			Volume = 0.5f,
			IsLooped = true,
		};
		Conditions = new TagCondition[] {
			new(TagCondition.ConditionType.Any, "Corruption", "Crimson", "Meteor", "Dungeon"),
		};
	}
}

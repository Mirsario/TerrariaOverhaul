using System;
using TerrariaOverhaul.Common.Ambience;

namespace TerrariaOverhaul.Core.Tags;

public struct TagCondition
{
	public enum ConditionType
	{
		All,
		Any,
		None,
	}

	public ConditionType Type = ConditionType.All;
	public Tag[] Tags = Array.Empty<Tag>();

	public TagCondition(ConditionType type, params Tag[] tags)
	{
		Type = type;
		Tags = tags;
	}

	public bool Check()
	{
		(bool defaultValue, bool valueForNonDefault) = Type switch {
			ConditionType.All => (true, false),
			ConditionType.Any => (false, true),
			ConditionType.None => (true, true),
			_ => throw new InvalidOperationException(),
		};

		for (int i = 0; i < Tags.Length; i++) {
			if (EnvironmentSystem.HasTag(Tags[i]) == valueForNonDefault) {
				return !defaultValue;
			}
		}

		return defaultValue;
	}
}

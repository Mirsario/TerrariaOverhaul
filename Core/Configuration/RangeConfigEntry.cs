using System;

namespace TerrariaOverhaul.Core.Configuration;

public class RangeConfigEntry<T> : ConfigEntry<T> where T : IComparable<T>
{
	public T MinValue { get; }
	public T MaxValue { get; }

	public RangeConfigEntry(ConfigSide side, string categoryId, string nameId, T minValue, T maxValue, Func<T> defaultValueGetter) : base(side, categoryId, nameId, defaultValueGetter)
	{
		int comparison = minValue.CompareTo(maxValue);

		if (comparison > 0) {
			throw new ArgumentException($"Minimal value must be less than or equal to maximum value.");
		}

		MinValue = minValue;
		MaxValue = maxValue;
		// Re-run a part of the original constructor. Inheritance is lovely, isn't it?
		RemoteValue = DefaultValue;
		LocalValue = DefaultValue;
	}

	protected override T? ModifySetValue(T? value)
	{
		value = base.ModifySetValue(value);

		if (MinValue.CompareTo(value) > 0) {
			value = MinValue;
		} else if (MaxValue.CompareTo(value) < 0) {
			value = MaxValue;
		}

		return value;
	}
}

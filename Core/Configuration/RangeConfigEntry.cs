// Copyright (c) 2020-2025 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using System;

namespace TerrariaOverhaul.Core.Configuration;

public class RangeConfigEntry<T> : ConfigEntry<T> where T : IComparable<T>
{
	public T MinValue { get; }
	public T MaxValue { get; }

	public RangeConfigEntry(ConfigSide side, T defaultValue, (T minValue, T maxValue) range, params string[] categories) : base(side, defaultValue, categories)
	{
		int comparison = range.minValue.CompareTo(range.maxValue);

		if (comparison > 0) {
			throw new ArgumentException($"Minimal value must be less than or equal to maximum value.");
		}

		MinValue = range.minValue;
		MaxValue = range.maxValue;
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

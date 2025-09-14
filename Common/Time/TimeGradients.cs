// Copyright (c) 2020-2025 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using TerrariaOverhaul.Utilities.Xna;

namespace TerrariaOverhaul.Common.Time;

internal static class TimeGradients
{
	public static readonly Gradient<float> Day = new(
		(TimeOfDay.HoursToTicks(0.00f), 0.00f),
		(TimeOfDay.HoursToTicks(5.00f), 0.00f),
		(TimeOfDay.HoursToTicks(7.00f), 1.00f),
		(TimeOfDay.HoursToTicks(17.5f), 1.00f),
		(TimeOfDay.HoursToTicks(21.0f), 0.00f),
		(TimeOfDay.HoursToTicks(24.0f), 0.00f)
	);

	public static readonly Gradient<float> Night = new(
		(TimeOfDay.HoursToTicks(0.00f), 1.00f),
		(TimeOfDay.HoursToTicks(5.00f), 1.00f),
		(TimeOfDay.HoursToTicks(7.00f), 0.00f),
		(TimeOfDay.HoursToTicks(17.5f), 0.00f),
		(TimeOfDay.HoursToTicks(21.0f), 1.00f),
		(TimeOfDay.HoursToTicks(24.0f), 1.00f)
	);
}

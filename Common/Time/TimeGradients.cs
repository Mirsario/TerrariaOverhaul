using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.Time;

public static class TimeGradients
{
	public static readonly Gradient<float> Day = new(
		(TimeOfDay.HoursToTicks(0.00f), 0.00f),
		(TimeOfDay.HoursToTicks(6.00f), 0.00f),
		(TimeOfDay.HoursToTicks(9.00f), 1.00f),
		(TimeOfDay.HoursToTicks(17.5f), 1.00f),
		(TimeOfDay.HoursToTicks(21.0f), 0.00f),
		(TimeOfDay.HoursToTicks(24.0f), 0.00f)
	);

	public static readonly Gradient<float> Night = new(
		(TimeOfDay.HoursToTicks(0.00f), 1.00f),
		(TimeOfDay.HoursToTicks(6.00f), 1.00f),
		(TimeOfDay.HoursToTicks(9.00f), 0.00f),
		(TimeOfDay.HoursToTicks(17.5f), 0.00f),
		(TimeOfDay.HoursToTicks(21.0f), 1.00f),
		(TimeOfDay.HoursToTicks(24.0f), 1.00f)
	);
}

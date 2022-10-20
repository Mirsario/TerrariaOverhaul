using Terraria;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.Time;

public static class TimeOfDay
{
	private const float DayHourCount = 24f;
	private static float DayHourOffset => -7.5f + 12f;

	public static readonly float DayLength = (float)Main.dayLength;
	public static readonly float NightLength = (float)Main.nightLength;
	public static readonly float FullDayLength = DayLength + NightLength;

	public static float InTicks => ((float)Main.time) + (Main.dayTime ? 0f : DayLength);
	public static float InHours => TicksToHours(InTicks);
	public static float AsFactor => TicksToFactor(InTicks);

	public static float TicksToHours(float ticks)
		=> FactorToHours(TicksToFactor(ticks));

	public static float TicksToFactor(float ticks)
		=> ticks / FullDayLength;

	public static float HoursToTicks(float hours)
		=> HoursToFactor(hours) * FullDayLength;

	public static float HoursToFactor(float hours)
		=> MathUtils.Modulo(hours - DayHourOffset, DayHourCount) / DayHourCount;

	public static float FactorToTicks(float factor)
		=> factor * FullDayLength;

	public static float FactorToHours(float factor)
		=> MathUtils.Modulo(factor * DayHourCount + DayHourOffset, DayHourCount);
}

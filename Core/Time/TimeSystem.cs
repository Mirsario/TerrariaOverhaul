using System;
using System.Diagnostics;
using Terraria;
using Terraria.ModLoader;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Core.Time
{
	public sealed class TimeSystem : ModSystem
	{
		public const float FullDayLength = (float)(Main.nightLength + Main.dayLength);
		public const float DayLength = 54000f;
		public const float NightLength = 32400f;
		public const int LogicFramerate = 60;
		public const float LogicDeltaTime = 1f / LogicFramerate;

		public static readonly DateTime FirstLoadDate = DateTime.Now;
		public static readonly Gradient<float> DayGradient = new(
			(0f, 0.125f),
			(14000f, 1f),
			(39000f, 1f),
			(49000f, 0f),
			(80000f, 0f),
			(FullDayLength, 0.125f)
		);
		public static readonly Gradient<float> NightGradient = new(
			(0f, 0.875f),
			(14000f, 0f),
			(39000f, 0f),
			(49000f, 1f),
			(80000f, 1f),
			(FullDayLength, 0.875f)
		);

		public static DateTime lastLoadDate;
		public static Stopwatch globalStopwatch;

		public static DateTime Date { get; } = DateTime.Now;
		public static bool AprilFools { get; private set; }
		public static bool AustraliaDay { get; private set; }
		public static bool ProgrammersDay { get; private set; }
		public static bool Halloween { get; private set; }
		public static bool Christmas { get; private set; }
		public static bool NewYear { get; private set; }
		public static ulong UpdateCount { get; private set; }
		public static double GlobalTime { get; private set; }

		public static float RealTime => (float)(Main.time + (Main.dayTime ? 0d : Main.dayLength));

		public override void Load()
		{
			// 1st April
			AprilFools = Date.Month == 4 && Date.Day == 1;
			// 26th January
			AustraliaDay = Date.Month == 1 && Date.Day == 26;
			// 13th or 12th September
			ProgrammersDay = Date.DayOfYear == 256;
			// From October 25th to November 5th, inclusively.
			Halloween = (Date.Month == 10 && Date.Day >= 25) || (Date.Month == 11 && Date.Day < 3);
			// From 24th December to 26th, inclusively.
			Christmas = Date.Month == 12 && Date.Day >= 24 && Date.Day <= 26;
			// From 27th December to 5th January, inclusively.
			NewYear = (Date.Month == 12 && Date.Day >= 27) || (Date.Month == 1 && Date.Day <= 5);

			lastLoadDate = DateTime.Now;

			globalStopwatch = new Stopwatch();
			globalStopwatch.Start();
		}

		public override void Unload()
		{
			globalStopwatch.Stop();
			globalStopwatch = null;
		}

		public override void PostUpdateEverything()
		{
			UpdateCount++;
			GlobalTime += LogicDeltaTime;
		}
	}
}

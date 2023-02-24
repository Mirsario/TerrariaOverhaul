using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace TerrariaOverhaul.Core.Time;

public sealed class TimeSystem : ModSystem
{
	public static readonly DateTime FirstLoadDate = DateTime.Now;

	private static uint lastRenderUpdateCount;

	public static DateTime Date { get; } = DateTime.Now;
	// Logic time
	public static float LogicTime { get; private set; }
	public static float LogicDeltaTime { get; } = 1f / 60f;
	public static int LogicFramerate { get; } = 60;
	// Render time
	public static float RenderTime { get; private set; }
	public static float RenderDeltaTime { get; private set; } = 1f / 60f;
	// Etc
	public static TimeSpan CurrentTimeSpan => GlobalStopwatch?.Elapsed ?? TimeSpan.Zero;
	public static Stopwatch? GlobalStopwatch { get; private set; }
	public static ulong UpdateCount { get; private set; }
	public static bool RenderOnlyFrame { get; private set; }
	// Events
	public static bool AprilFools { get; private set; }
	public static bool AustraliaDay { get; private set; }
	public static bool ProgrammersDay { get; private set; }
	public static bool Halloween { get; private set; }
	public static bool Christmas { get; private set; }
	public static bool NewYear { get; private set; }

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

		GlobalStopwatch = Stopwatch.StartNew();

		// Hooking of potentially executing draw methods has to be done on the main thread.
		Main.QueueMainThreadAction(static () => {
			On_Main.DoUpdate += OnDoUpdate;

			On_Main.DoDraw += OnDoDraw;
		});
	}

	private static void OnDoDraw(On_Main.orig_DoDraw orig, Main main, GameTime gameTime)
	{
		uint updateCount = Main.GameUpdateCount;

		RenderOnlyFrame = updateCount == lastRenderUpdateCount;
		lastRenderUpdateCount = updateCount;

		RenderTime = (float)gameTime.TotalGameTime.TotalSeconds;
		RenderDeltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

		orig(main, gameTime);

		RenderOnlyFrame = false;
	}

	private static void OnDoUpdate(On_Main.orig_DoUpdate orig, Main main, ref GameTime gameTime)
	{
		LogicTime = (float)gameTime.TotalGameTime.TotalSeconds;
		//LogicDeltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

		orig(main, ref gameTime);
	}

	public override void Unload()
	{
		GlobalStopwatch?.Stop();

		GlobalStopwatch = null;
	}

	public override void PostUpdateEverything()
	{
		UpdateCount++;
	}
}

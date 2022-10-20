using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.Time;
using TerrariaOverhaul.Core.Time;
using TerrariaOverhaul.Utilities;

#pragma warning disable IDE0060 // Remove unused parameter

namespace TerrariaOverhaul.Common.Ambience;

public sealed class VolumeMultiplier
{
	public struct Context
	{
		public Player Player;
		public Vector2 PlayerTilePosition;
	}

	public delegate float Function(in Context context);

	// Time

	public static float DayTime(in Context context)
		=> TimeGradients.Day.GetValue(TimeOfDay.InTicks);

	public static float NightTime(in Context context)
		=> TimeGradients.Night.GetValue(TimeOfDay.InTicks);

	// Altitude

	public static float SurfaceOrSkyAltitude(in Context context)
		=> WorldLocationUtils.SurfaceOrSkyGradient.GetValue(context.PlayerTilePosition.Y);

	public static float SurfaceAltitude(in Context context)
		=> WorldLocationUtils.SurfaceGradient.GetValue(context.PlayerTilePosition.Y);

	public static float UnderSurfaceAltitude(in Context context)
		=> WorldLocationUtils.UnderSurfaceGradient.GetValue(context.PlayerTilePosition.Y);

	public static float SpaceGradientAltitude(in Context context)
		=> WorldLocationUtils.SpaceGradient.GetValue(context.PlayerTilePosition.Y);

	// Weather

	public static float RainWeather(in Context context)
		=> MathHelper.Clamp(Main.maxRaining * 2f, 0f, 1f);

	public static float NotRainWeather(in Context context)
		=> MathHelper.Clamp(1f - (Main.maxRaining * 2f), 0f, 1f);
}

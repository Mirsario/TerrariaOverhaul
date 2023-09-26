using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using TerrariaOverhaul.Common.Time;
using TerrariaOverhaul.Utilities;

#pragma warning disable IDE0060 // Remove unused parameter
#pragma warning disable IDE0051 // Remove unused private members

namespace TerrariaOverhaul.Common.Ambience;

public static class EnvironmentSignals
{
	// Time

	[EnvironmentSignalUpdater]
	private static float DayTime(in EnvironmentContext context)
		=> TimeGradients.Day.GetValue(TimeOfDay.InTicks);

	[EnvironmentSignalUpdater]
	private static float NightTime(in EnvironmentContext context)
		=> TimeGradients.Night.GetValue(TimeOfDay.InTicks);

	// Altitude

	[EnvironmentSignalUpdater]
	private static float SurfaceOrSkyAltitude(in EnvironmentContext context)
		=> WorldLocationUtils.SurfaceOrSkyGradient.GetValue(context.PlayerTilePosition.Y);

	[EnvironmentSignalUpdater]
	private static float SurfaceAltitude(in EnvironmentContext context)
		=> WorldLocationUtils.SurfaceGradient.GetValue(context.PlayerTilePosition.Y);

	[EnvironmentSignalUpdater]
	private static float UnderSurfaceAltitude(in EnvironmentContext context)
		=> WorldLocationUtils.UnderSurfaceGradient.GetValue(context.PlayerTilePosition.Y);

	[EnvironmentSignalUpdater]
	private static float SpaceAltitude(in EnvironmentContext context)
		=> WorldLocationUtils.SpaceGradient.GetValue(context.PlayerTilePosition.Y);

	// Weather

	[EnvironmentSignalUpdater]
	private static float RainWeather(in EnvironmentContext context)
		=> MathHelper.Clamp(Main.maxRaining * 2f, 0f, 1f);

	[EnvironmentSignalUpdater]
	private static float NotRainWeather(in EnvironmentContext context)
		=> 1f - RainWeather(in context);

	// Nature

	[EnvironmentSignalUpdater]
	private static float TreesAround(in EnvironmentContext context)
		=> MathHelper.Clamp(context.TileCounts[TileID.Trees] / 300f, 0f, 1f);

	[EnvironmentSignalUpdater]
	private static float TreesNotAround(in EnvironmentContext context)
		=> 1f - TreesAround(in context);

	// Biomes

	private static float CalculateBiomeFactor(int tileCount, int threshold, int max)
		=> (tileCount - threshold) / (float)(max - threshold);

	[EnvironmentSignalUpdater]
	private static float Corruption(in EnvironmentContext context)
		=> CalculateBiomeFactor(context.Metrics.EvilTileCount, SceneMetrics.CorruptionTileThreshold, SceneMetrics.CorruptionTileMax);

	[EnvironmentSignalUpdater]
	private static float Crimson(in EnvironmentContext context)
		=> CalculateBiomeFactor(context.Metrics.BloodTileCount, SceneMetrics.CrimsonTileThreshold, SceneMetrics.CrimsonTileMax);

	[EnvironmentSignalUpdater]
	private static float Hallow(in EnvironmentContext context)
		=> CalculateBiomeFactor(context.Metrics.HolyTileCount, SceneMetrics.HallowTileThreshold, SceneMetrics.HallowTileMax);

	[EnvironmentSignalUpdater]
	private static float Desert(in EnvironmentContext context)
		=> CalculateBiomeFactor(context.Metrics.SandTileCount, 0, SceneMetrics.DesertTileThreshold);

	[EnvironmentSignalUpdater]
	private static float Jungle(in EnvironmentContext context)
		=> CalculateBiomeFactor(context.Metrics.JungleTileCount, SceneMetrics.JungleTileThreshold, SceneMetrics.JungleTileMax);

	[EnvironmentSignalUpdater]
	private static float Tundra(in EnvironmentContext context)
		=> CalculateBiomeFactor(context.Metrics.SnowTileCount, SceneMetrics.SnowTileThreshold, SceneMetrics.SnowTileMax);
}

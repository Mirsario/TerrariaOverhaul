// Copyright (c) 2020-2025 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Utilities.Terraria;
using EnvironmentTag = TerrariaOverhaul.Core.Tags.Tag<TerrariaOverhaul.Common.Ambience.EnvironmentSystem>;

namespace TerrariaOverhaul.Common.Ambience;

[AttributeUsage(AttributeTargets.Method)]
internal sealed class EnvironmentSignalUpdaterAttribute(string? tagNameOverride = null) : Attribute
{
	public readonly string? TagNameOverride = tagNameOverride;
}

internal readonly ref struct EnvironmentContext
{
	public Player Player { get; init; }
	public Vector2 PlayerTilePosition { get; init; }
	public ReadOnlySpan<int> TileCounts { get; init; }
	public SceneMetrics Metrics { get; init; }
}

/// <summary>
/// Utility system for setting and getting dynamic tags of the local player's environment.
/// </summary>
internal sealed partial class EnvironmentSystem : ModSystem
{
	public delegate float SignalUpdater(in EnvironmentContext context);

	private static readonly Dictionary<EnvironmentTag, float> environmentSignals = new();
	private static readonly List<(EnvironmentTag tag, SignalUpdater function)> signalUpdaters = new();
	private static readonly EnvironmentTag[,] biomeTagsByMaskIndex = new EnvironmentTag[4, 8];

	private static int[]? tileCounts;

	public override void Load()
	{
		FillZoneBitmaskMapping(biomeTagsByMaskIndex);

		foreach (var type in Assembly.GetExecutingAssembly().GetTypes()) {
			foreach (var method in type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)) {
				var attribute = method.GetCustomAttribute<EnvironmentSignalUpdaterAttribute>();

				if (attribute == null) {
					continue;
				}

				var function = method.CreateDelegate<SignalUpdater>();

				RegisterSignalUpdater(attribute.TagNameOverride ?? method.Name, function);
			}
		}

		// Biomes
		RegisterSignalUpdater("Purity", static (in EnvironmentContext _) => Main.LocalPlayer.ZonePurity ? 1f : 0f);
		RegisterSignalUpdater("Forest", static (in EnvironmentContext _) => Main.LocalPlayer.ZoneForest ? 1f : 0f);
		RegisterSignalUpdater("NormalSpace", static (in EnvironmentContext _) => Main.LocalPlayer.ZoneNormalSpace ? 1f : 0f);
		RegisterSignalUpdater("NormalCaverns", static (in EnvironmentContext _) => Main.LocalPlayer.ZoneNormalCaverns ? 1f : 0f);
		RegisterSignalUpdater("NormalUnderground", static (in EnvironmentContext _) => Main.LocalPlayer.ZoneNormalUnderground ? 1f : 0f);
	}

	public override void TileCountsAvailable(ReadOnlySpan<int> tileCountsSpan)
	{
		Array.Resize(ref tileCounts, tileCountsSpan.Length);
		tileCountsSpan.CopyTo(tileCounts);
	}

	public override void PostUpdatePlayers()
	{
		if (Main.netMode == NetmodeID.Server) {
			return;
		}

		if (tileCounts == null) {
			return;
		}

		var localPlayer = Main.LocalPlayer;
		var context = new EnvironmentContext {
			Player = Main.LocalPlayer,
			PlayerTilePosition = localPlayer.Center * WorldUtils.PixelSizeInUnits,
			TileCounts = tileCounts,
			Metrics = Main.SceneMetrics,
		};

		// Update zone bits tags
		ReadOnlySpan<BitsByte> bitsBytes = [
			localPlayer.zone1,
			localPlayer.zone2,
			localPlayer.zone3,
			localPlayer.zone4,
		];

		for (int i = 0; i < bitsBytes.Length; i++) {
			var bitsByte = bitsBytes[i];

			for (int j = 0; j < 8; j++) {
				var tag = biomeTagsByMaskIndex[i, j];

				if (tag != default) {
					SetSignal(tag, bitsByte[j] ? 1f : 0f);
				}
			}
		}

		// Invoke signal updaters.
		foreach (var (tag, function) in signalUpdaters) {
			SetSignal(tag, function(in context));
		}
	}

	public static void RegisterSignalUpdater(EnvironmentTag tag, SignalUpdater function)
		=> signalUpdaters.Add((tag, function));

	public static bool TryGetSignal(EnvironmentTag tag, out float signal)
	{
		return environmentSignals.TryGetValue(tag, out signal);
	}

	public static float GetSignal(EnvironmentTag tag)
	{
		TryGetSignal(tag, out float signal);

		return signal;
	}

	public static void SetSignal(EnvironmentTag tag, float value)
	{
		if (tag == default) {
			return;
		}

		if (value > 0f) {
			environmentSignals[tag] = MathHelper.Clamp(value, 0f, 1f);
			return;
		}

		environmentSignals.Remove(tag);
	}

	private static void FillZoneBitmaskMapping(EnvironmentTag[,] map)
	{
		// Zone1
		map[0, 0] = "Dungeon";
		map[0, 1] = "Corruption";
		map[0, 2] = "Hallow";
		map[0, 3] = "Meteor";
		map[0, 4] = "Jungle";
		map[0, 5] = "Snow";
		map[0, 6] = "Crimson";
		map[0, 7] = "WaterCandle";
		// Zone2
		map[1, 0] = "PeaceCandle";
		map[1, 1] = "TowerSolar";
		map[1, 2] = "TowerVortex";
		map[1, 3] = "TowerNebula";
		map[1, 4] = "TowerStardust";
		map[1, 5] = "Desert";
		map[1, 6] = "Glowshroom";
		map[1, 7] = "UndergroundDesert";
		// Zone3
		map[2, 0] = "SkyHeight";
		map[2, 1] = "OverworldHeight";
		map[2, 2] = "DirtLayerHeight";
		map[2, 3] = "RockLayerHeight";
		map[2, 4] = "UnderworldHeight";
		map[2, 5] = "Beach";
		map[2, 6] = "Rain";
		map[2, 7] = "Sandstorm";
		// Zone4
		map[3, 0] = "OldOneArmy";
		map[3, 1] = "Granite";
		map[3, 2] = "Marble";
		map[3, 3] = "Hive";
		map[3, 4] = "GemCave";
		map[3, 5] = "LihzhardTemple";
		map[3, 6] = "Graveyard";
		map[3, 7] = "ShadowCandle";
	}
}

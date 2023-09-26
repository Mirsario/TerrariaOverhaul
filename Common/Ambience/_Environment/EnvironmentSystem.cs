using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Core.Tags;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.Ambience;

/// <summary>
/// Utility system for setting and getting dynamic tags of the local player's environment.
/// </summary>
public sealed partial class EnvironmentSystem : ModSystem
{
	public delegate float SignalUpdater(in EnvironmentContext context);

	private static readonly Dictionary<Tag, float> environmentSignals = new();
	private static readonly List<(Tag tag, SignalUpdater function)> signalUpdaters = new();
	private static readonly List<Tag> biomeTagsById = new() {
		default, // zone1
		default, //"Dungeon",
		default, //"Corruption",
		default, //"Hallow",
		"Meteor",
		default, //"Jungle",
		default, //"Snow",
		default, //"Crimson",
		"WaterCandle",
		// zone2
		"PeaceCandle",
		"TowerSolar",
		"TowerVortex",
		"TowerNebula",
		"TowerStardust",
		default, //"Desert",
		"Glowshroom",
		"UndergroundDesert",
		// zone3
		"SkyHeight",
		"OverworldHeight",
		"DirtLayerHeight",
		"RockLayerHeight",
		"UnderworldHeight",
		"Beach",
		"Rain",
		"Sandstorm",
		// zone4
		"OldOneArmy",
		"Granite",
		"Marble",
		"Hive",
		"GemCave",
		"LihzhardTemple",
		"Graveyard",
	};

	private static int[]? tileCounts;

	public override void Load()
	{
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

	public override void PreUpdateWorld()
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
			PlayerTilePosition = localPlayer.Center * TileUtils.PixelSizeInUnits,
			TileCounts = tileCounts,
			Metrics = Main.SceneMetrics,
		};

		foreach (var (tag, function) in signalUpdaters) {
			SetSignal(tag, function(in context));
		}

		// Update zone bits tags
		ReadOnlySpan<BitsByte> bitsBytes = stackalloc BitsByte[4] {
			localPlayer.zone1,
			localPlayer.zone2,
			localPlayer.zone3,
			localPlayer.zone4,
		};

		for (int i = 0, globalId = 0; i < bitsBytes.Length; i++) {
			var bitsByte = bitsBytes[i];

			for (int j = 0; j < 8 && globalId < biomeTagsById.Count; j++, globalId++) {
				var tag = biomeTagsById[globalId];

				if (tag != default) {
					SetSignal(tag, bitsByte[j] ? 1f : 0f);
				}
			}
		}
	}

	public static void RegisterSignalUpdater(Tag tag, SignalUpdater function)
		=> signalUpdaters.Add((tag, function));


	public static bool TryGetSignal(Tag tag, out float signal)
	{
		return environmentSignals.TryGetValue(tag, out signal);
	}

	public static float GetSignal(Tag tag)
	{
		TryGetSignal(tag, out float signal);

		return signal;
	}

	public static void SetSignal(Tag tag, float value)
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
}

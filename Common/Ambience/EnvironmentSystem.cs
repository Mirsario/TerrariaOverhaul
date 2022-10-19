using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using TerrariaOverhaul.Core.Tags;

namespace TerrariaOverhaul.Common.Ambience;

/// <summary>
/// Utility system for setting and getting dynamic tags of the local player's environment.
/// </summary>
public sealed class EnvironmentSystem : ModSystem
{
	private static readonly HashSet<Tag> environmentTags = new();
	private static readonly List<(Tag tag, Func<bool> function)> tagUpdaters = new();
	private static readonly List<Tag> biomeTagsById = new() {
		// zone1
		"Dungeon",
		"Corruption",
		"Hallow",
		"Meteor",
		"Jungle",
		"Snow",
		"Crimson",
		"WaterCandle",
		// zone2
		"PeaceCandle",
		"TowerSolar",
		"TowerVortex",
		"TowerNebula",
		"TowerStardust",
		"Desert",
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

	public override void Load()
	{
		// Biomes
		RegisterTagUpdater("Purity", static () => Main.LocalPlayer.ZonePurity);
		RegisterTagUpdater("Forest", static () => Main.LocalPlayer.ZoneForest);
		RegisterTagUpdater("NormalCaverns", static () => Main.LocalPlayer.ZoneNormalCaverns);
		RegisterTagUpdater("NormalUnderground", static () => Main.LocalPlayer.ZoneNormalUnderground);
		RegisterTagUpdater("NormalSpace", static () => Main.LocalPlayer.ZoneNormalSpace);
	}

	public override void PreUpdateWorld()
	{
		foreach (var (tag, function) in tagUpdaters) {
			SetTag(tag, function());
		}

		// Update zone bits tags
		var player = Main.LocalPlayer;
		ReadOnlySpan<BitsByte> bitsBytes = stackalloc BitsByte[4] {
			player.zone1,
			player.zone2,
			player.zone3,
			player.zone4,
		};

		for (int i = 0, globalId = 0; i < bitsBytes.Length; i++) {
			var bitsByte = bitsBytes[i];

			for (int j = 0; j < 8 && globalId < biomeTagsById.Count; j++, globalId++) {
				SetTag(biomeTagsById[globalId], bitsByte[j]);
			}
		}
	}

	public static void RegisterTagUpdater(Tag tag, Func<bool> function)
	{
		tagUpdaters.Add((tag, function));
	}

	public static bool HasTag(Tag tag)
		=> environmentTags.Contains(tag);

	public static bool SetTag(Tag tag, bool value)
	{
		if (value) {
			return environmentTags.Add(tag);
		}

		return environmentTags.Remove(tag);
	}
}

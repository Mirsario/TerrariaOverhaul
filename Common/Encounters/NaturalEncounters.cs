// Copyright (c) 2020-2025 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Utilities.Terraria;
using TerrariaOverhaul.Utilities.Xna;

namespace TerrariaOverhaul.Common.Encounters;

internal sealed class NaturalEncounters : ModSystem
{
	public override void PreUpdateEntities()
	{
		if (Main.netMode == NetmodeID.MultiplayerClient) return;

		if (EnemyEncounters.EnableEnemyEncounters) {
			GenerateNaturalEncounters();
			CleanupCompletedEncounters();
		}
	}

	private static void GenerateNaturalEncounters()
	{
		const int MaxAttempts = 50;
		const int MaxNewEncountersPerTick = 1;
		const float MinSqrDistanceFromOtherEncounters = 4096f * 4096f;
		const float MinSqrDistanceFromPlayers = 2048f * 2048f;
		const int OffsetFromEdges = 16;
		const int TileExtension = 32;

		var minFreeSpace = new Vector2Int(12, 7);
		int targetEncounterCount = 100;
		int newEncounters = 0;
		var waves = new List<EncounterWave>();
		var spawns = new List<EnemySpawn>();

		for (int attempt = 0; EnemyEncounters.Count < targetEncounterCount && attempt < MaxAttempts; attempt++) {
			var spawnOrigin = new Point16(
				Main.rand.Next(OffsetFromEdges, Main.maxTilesX - OffsetFromEdges),
				Main.rand.Next(OffsetFromEdges, Main.maxTilesY - OffsetFromEdges)
			);

			// Reroll right away if this is a solid block.
			var baseTile = Main.tile[spawnOrigin.X, spawnOrigin.Y];
			if (baseTile.HasUnactuatedTile && Main.tileSolid[baseTile.TileType]) {
				continue;
			}

			// Ensure that the base position has enough space.
			if (!WorldUtils.TryFitRectangleIntoTilemap(spawnOrigin, minFreeSpace, out _)) {
				continue;
			}

			// Move the encounter center to the ground.
			for (int yy = spawnOrigin.Y; yy < Main.maxTilesY + 1; yy++) {
				var tile = Main.tile[spawnOrigin.X, yy];
				if (tile.HasUnactuatedTile && Main.tileSolid[tile.TileType]) {
					spawnOrigin = new Point16(spawnOrigin.X, yy - 1);
					break;
				}
			}

			// Ensure that the ground position also has enough space.
			if (!WorldUtils.TryFitRectangleIntoTilemap(spawnOrigin, minFreeSpace, out _)) {
				continue;
			}

			// Go up to the center of our space check, so that slopes do not bother other algorithms.
			spawnOrigin = new Point16(spawnOrigin.X, spawnOrigin.Y - (minFreeSpace.Y / 2));

			// Ensure that this encounter is not created too close to another.
			var activationOrigin = new Point(spawnOrigin.X, spawnOrigin.Y).ToWorldCoordinates();

			foreach (ref readonly var instance in EnemyEncounters.Encounters) {
				if (instance.Encounter.ActivationOrigin.DistanceSQ(activationOrigin) <= MinSqrDistanceFromOtherEncounters) {
					goto Continue;
				}
			}

			// Ensure that this encounter is not created too close to a player.
			foreach (var player in Main.ActivePlayers) {
				if (player.Center.DistanceSQ(activationOrigin) <= MinSqrDistanceFromPlayers) {
					goto Continue;
				}
			}

			// Finalize position calculations.
			var spawnArea = new Vector4Int(
				Math.Max(0, spawnOrigin.X - TileExtension),
				Math.Max(0, spawnOrigin.Y - (int)(TileExtension * 0.75f)),
				Math.Min(Main.maxTilesX - 1, spawnOrigin.X + TileExtension),
				Math.Min(Main.maxTilesY - 1, spawnOrigin.Y + (int)(TileExtension * 0.25f))
			);
			var spawnRect = new Rectangle(spawnArea.X, spawnArea.Y, spawnArea.Z - spawnArea.X, spawnArea.W - spawnArea.Y);

			// Prepare the encounter.
			int numWaves = Main.rand.Next(1, 3 + 1);
			float activationRange = 512f;
			bool environmental = numWaves == 1;
			int music = -1;

			if (environmental) {
				activationRange *= 3f;
			} else {
				music = Main.rand.NextFromList(MusicID.Plantera, MusicID.Boss1, MusicID.Boss2, MusicID.Boss4);
			}

			// Fill waves.
			waves.Clear();
			waves.EnsureCapacity(numWaves);

			for (int waveIndex = 0; waveIndex < numWaves; waveIndex++) {
				spawns.Clear();

				// TEST CODE
				int[] npcPool = [NPCID.Zombie, NPCID.ArmedZombie, NPCID.BigZombie, NPCID.DemonEye];
				int numEnemies = Main.rand.Next(3, 6) * (waveIndex + 1);
				spawns.EnsureCapacity(numEnemies);
				for (int enemyIndex = 0; enemyIndex < numEnemies; enemyIndex++) {
					int npcType = npcPool[Main.rand.Next(npcPool.Length)];
					var npcSample = ContentSamples.NpcsByNetId[npcType];
					var checkSize = new Point16(npcSample.width, npcSample.height);

					var spawnPlacement = new SpawnPlacement {
						Area = spawnRect,
						AreaOrigin = spawnOrigin,
						CollisionSize = checkSize,
					};

					spawnPlacement.OnGround = npcSample.aiStyle == NPCAIStyleID.Fighter;
					spawnPlacement.SkippedLiquids = LiquidMask.All;

					var spawn = new EnemySpawn {
						NpcType = npcType,
						SpawnPosition = null,
						SpawnPlacement = spawnPlacement,
					};

					if (!environmental) {
						spawn.Effect = EnemySpawnEffect.Teleport;
						spawn.CooldownInTicks = (uint)Main.rand.Next(10, 20);
					}

					spawns.Add(spawn);
				}

				waves.Add(new EncounterWave {
					Spawns = spawns.ToArray(),
				});
			}

			// Create the encounter.
			EnemyEncounters.CreateEncounter(new Encounter {
				Identifier = "NaturalEncounter",
				ActivationOrigin = activationOrigin,
				ActivationRange = activationRange,
				SpawnOrigin = spawnOrigin,
				SpawnArea = spawnRect,
				Waves = waves.ToArray(),
				// Scene Effects.
				MusicIndex = music,
				SceneEffectPriority = SceneEffectPriority.Environment,
			});

			// Stop if this is enough.
			if (++newEncounters >= MaxNewEncountersPerTick) {
				break;
			}

			Continue:;
		}
	}

	private static void CleanupCompletedEncounters()
	{

	}
}

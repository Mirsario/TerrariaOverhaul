// Copyright (c) 2020-2025 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;
using TerrariaOverhaul.Utilities;
using TerrariaOverhaul.Utilities.Terraria;
using TerrariaOverhaul.Utilities.Xna;

namespace TerrariaOverhaul.Common.Encounters;

internal sealed class NaturalEncounters : ModSystem
{
	private static uint vanillaSpawnLogicStack;
	private static GlobalHookList<GlobalNPC> hookEditSpawnPool = null!;

	private static bool IsInVanillaSpawnLogic => vanillaSpawnLogicStack != 0;

	public override void Load()
	{
		IL_Main.DoUpdateInWorld += DoUpdateInWorldInjection;
		IL_NPC.NewNPC += NewNPCInjection;

		hookEditSpawnPool = (GlobalHookList<GlobalNPC>)typeof(NPCLoader).GetField("HookEditSpawnPool", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public)!.GetValue(null)!;
	}

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
		const int MaxNewEncountersPerTick = 3;
		const float MinSqrDistanceFromOtherEncountersLight = 1200f * 1200f;
		const float MinSqrDistanceFromOtherEncountersHeavy = 2500f * 2500f;
		const float MinSqrDistanceFromPlayers = 2100f * 2100f;
		const int OffsetFromEdges = 16;
		const int TileExtension = 32;

		var minFreeSpace = new Vector2Int(12, 7);
		int newEncounters = 0;

		int numLightEncounters = 0;
		int numHeavyEncounters = 0;
		int targetLightEncounters = 200;
		int targetHeavyEncounters = 10;
		const string IdLight = "Natural_Light";
		const string IdHeavy = "Natural_Heavy";

		foreach (ref readonly var instance in EnemyEncounters.Encounters) {
			ref readonly var encounter = ref instance.Encounter;

			if (ReferenceEquals(encounter.Identifier, IdLight)) {
				numLightEncounters++;
			} else if (ReferenceEquals(encounter.Identifier, IdHeavy)) {
				numHeavyEncounters++;
			}
		}

		for (int attempt = 0; attempt < MaxAttempts; attempt++) {
			int encounterType = numHeavyEncounters < targetHeavyEncounters ? 1 : 0;
			
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

			var activationOrigin = new Point(spawnOrigin.X, spawnOrigin.Y).ToWorldCoordinates();

			// Ensure that this encounter is not created too close to another.
			foreach (ref readonly var instance in EnemyEncounters.Encounters) {
				float sqrRange = encounterType == 0 ? MinSqrDistanceFromOtherEncountersLight : MinSqrDistanceFromOtherEncountersHeavy;

				if (instance.Encounter.ActivationOrigin.DistanceSQ(activationOrigin) <= sqrRange) {
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
			int numWaves = encounterType switch {
				1 => 3,
				_ => 1,
			};
			float activationRange = 512f;
			bool environmental = numWaves == 1;
			int music = -1;

			if (environmental) {
				activationRange = 1250f;
			} else {
				music = Main.rand.NextFromList(MusicID.Plantera, MusicID.Boss1, MusicID.Boss2, MusicID.Boss4);
			}

			// Create dummy waves.
			var waves = new EncounterWave[numWaves];

			foreach (ref var wave in (Span<EncounterWave>)waves) {
				wave = new EncounterWave {
					Spawns = [],
				};
			}

			// Create the encounter.
			EnemyEncounters.CreateEncounter(new Encounter {
				Identifier = encounterType == 1 ? IdHeavy : IdLight,
				ActivationOrigin = activationOrigin,
				ActivationRange = activationRange,
				SpawnOrigin = spawnOrigin,
				SpawnArea = spawnRect,
				Waves = waves,
				// Scene Effects.
				MusicIndex = music,
				SceneEffectPriority = SceneEffectPriority.Environment,
				// Callbacks
				OnActivated = OnNaturalEncounterStarted,
			});

			// Stop if this is enough.
			if (++newEncounters >= MaxNewEncountersPerTick) {
				break;
			}

			Continue:;
		}
	}

	private static void OnNaturalEncounterStarted(ref Encounter encounter, in EncounterContext ctx)
	{
		int numWaves = encounter.Waves.Length;
		bool environmental = numWaves == 1;
		var spawns = new List<EnemySpawn>();
		var player = ctx.Player;

		// Acquire mod pool.

		/*
		var modPool = new Dictionary<int, float>();
		var spawnInfo = new NPCSpawnInfo {
			Player = player,
			SpawnTileX = encounter.SpawnOrigin.X,
			SpawnTileY = encounter.SpawnOrigin.Y,
			SpawnTileType = Main.tile[encounter.SpawnOrigin.X, encounter.SpawnOrigin.Y].TileType,
			Sky = player.ZoneSkyHeight,
		};

		foreach (var g in hookEditSpawnPool.Enumerate()) {
			g.EditSpawnPool(modPool, spawnInfo);
		}
		*/

		// Build enemy pool.

		var pool = new List<int>();

		//pool.AddRange(modPool.Keys.Where(k => k != 0));

		if (player.ZoneDirtLayerHeight || player.ZoneRockLayerHeight) {
			pool.Add(NPCID.Skeleton);
			pool.Add(NPCID.CaveBat);
		}

		if (player.ZoneSkyHeight) {
			pool.Add(NPCID.Harpy);
		}

		if (player.ZoneSnow) {
			if (player.ZoneOverworldHeight) {
				pool.Add(NPCID.IceSlime);

				if (!Main.dayTime) {
					pool.Add(NPCID.ZombieEskimo);
					pool.Add(NPCID.ArmedZombieEskimo);
					pool.Add(NPCID.IceSlime);
				}
			}
		}

		if (player.ZoneOverworldHeight) {
			if (!Main.dayTime) {
				pool.Add(NPCID.DemonEye);
			}
		}

		if (player.ZoneCrimson) {
			pool.Add(NPCID.FaceMonster);
			pool.Add(NPCID.FaceMonster);
			pool.Add(NPCID.FaceMonster);
			pool.Add(NPCID.BigCrimera);
			pool.Add(NPCID.Crimera);
			pool.Add(NPCID.LittleCrimera);
		}

		if (player.ZoneForest || pool.Count == 0) {
			if (Main.dayTime) {
				pool.Add(NPCID.BlueSlime);
				pool.Add(NPCID.GreenSlime);
			} else {
				pool.Add(NPCID.Zombie);
				pool.Add(NPCID.ArmedZombie);
				pool.Add(NPCID.BigZombie);
			}
		}

		// Build waves.

		for (int waveIndex = 0; waveIndex < numWaves; waveIndex++) {
			spawns.Clear();

			// TEST CODE
			int numEnemies = Main.rand.Next(3, 6) * (waveIndex + 1);

			spawns.EnsureCapacity(numEnemies);

			for (int enemyIndex = 0; enemyIndex < numEnemies; enemyIndex++) {
				int npcType = pool[Main.rand.Next(pool.Count)];
				var npcSample = ContentSamples.NpcsByNetId[npcType];
				var checkSize = new Point16(npcSample.width, npcSample.height);

				var spawnPlacement = new SpawnPlacement {
					Area = default, // This will be substitued by base encounter code.
					AreaOrigin = default, // This will be substitued by base encounter code.
					CollisionSize = checkSize,
				};

				spawnPlacement.OnGround = npcSample.aiStyle is NPCAIStyleID.Fighter or NPCAIStyleID.Slime;
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

			encounter.Waves[waveIndex].Spawns = spawns.ToArray();
		}
	}

	private static void CleanupCompletedEncounters()
	{

	}

	private static bool IsNpcAllowedInVanillaSpawnLogic(int type)
	{
		if (!EnemyEncounters.EnableEnemyEncounters || !IsInVanillaSpawnLogic) {
			return true;
		}

		var sample = ContentSamples.NpcsByNetId[type];

		// Allow everything during invasions.
		if (Main.invasionType != 0) {
			return true;
		}

		// Allow town NPCs, critters, and everything 'rare'.
		if (sample.townNPC || NPCID.Sets.CountsAsCritter[type] || NPCID.Sets.TownCritter[type] || sample.rarity > 0) {
			return true;
		}

		// Allow everything invincible.
		if (sample.dontTakeDamage) {
			return true;
		}

		// Forbid hostiles.
		if (!sample.friendly && sample.damage > 0) {
			return false;
		}

		return true;
	}

	private static bool PreVanillaSpawnLogic()
	{
		vanillaSpawnLogicStack = checked(vanillaSpawnLogicStack + 1);

		return false;
	}

	private static void PostVanillaSpawnLogic()
	{
		vanillaSpawnLogicStack = checked(vanillaSpawnLogicStack - 1);
	}

	private static void DoUpdateInWorldInjection(ILContext ctx)
	{
		var il = new ILCursor(ctx);

		// Match 'NPC.SpawnNPC() in a try block'.
		ILLabel? tryBlockEnd = null!;
		il.GotoNext(MoveType.Before,
			i => i.MatchNop(),
			i => i.MatchCall(typeof(NPC), nameof(NPC.SpawnNPC)),
			i => i.MatchLeave(out tryBlockEnd)
		);

		// Before the try block.
		ILUtils.HijackIncomingLabels(il);
		var skipVanillaSpawnLogicLabel = il.DefineLabel();
		il.EmitDelegate(PreVanillaSpawnLogic);
		il.EmitBrtrue(skipVanillaSpawnLogicLabel);

		// After the try block.
		il.GotoNext(MoveType.AfterLabel, i => i == tryBlockEnd.Target);
		il.EmitNop();
		skipVanillaSpawnLogicLabel.Target = il.Prev;
		il.EmitDelegate(PostVanillaSpawnLogic);
	}

	private static void NewNPCInjection(ILContext ctx)
	{
		var il = new ILCursor(ctx);

		var skipReturnLabel = il.DefineLabel();
		il.Emit(OpCodes.Ldarg_3);
		il.EmitDelegate(IsNpcAllowedInVanillaSpawnLogic);
		il.EmitBrtrue(skipReturnLabel);
		il.EmitLdsfld(typeof(Main).GetField(nameof(Main.maxNPCs), BindingFlags.Static | BindingFlags.Public)!);
		il.EmitRet();
		il.MarkLabel(skipReturnLabel);
	}
}

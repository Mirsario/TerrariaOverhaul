using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using MonoMod.Cil;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Core.Configuration;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.Encounters;

internal delegate void EncounterCallback(ref Encounter encounter, in EncounterContext ctx);

/// <summary> Description used to create an enemy encounter. </summary>
internal struct Encounter()
{
	/// <summary> An identifier for this encounter. Does not do anything on its own. </summary>
	public required string Identifier { get; set; }
	/// <summary> The encounter's activation center in world-space (pixels). </summary>
	public required Vector2 ActivationOrigin { get; set; }
	/// <summary> The encounter's activation range in world-space (pixels). </summary>
	public required float ActivationRange { get; set; }
	/// <summary> The encounter's spawn area in tile-space. This will be supplied to all spawns with zeroed <see cref="SpawnPlacement.Area"/>. </summary>
	public required Rectangle SpawnArea { get; set; }
	/// <summary> The encounter's spawn center in tile-space. This will be supplied to all spawns with zeroed <see cref="SpawnPlacement.AreaOrigin"/>. </summary>
	public required Point16 SpawnOrigin { get; set; }
	/// <summary> The waves that this encounter consists of. </summary>
	public required EncounterWave[] Waves { get; set; }
	/// <summary> If set together with <see cref="SceneEffectPriority"/>, overrides played music when active and near. </summary>
	public int MusicIndex { get; set; } = -1;
	/// <summary> The priority to use for effects such as <see cref="MusicIndex"/>. </summary>
	public SceneEffectPriority SceneEffectPriority { get; set; }

	/// <summary> Callback invoked when the encounter is started. </summary>
	public EncounterCallback? OnActivated { get; set; }
}

internal struct EncounterWave()
{
	/// <summary> The enemies to spawn. </summary>
	public required EnemySpawn[] Spawns { get; set; }
}

internal struct EncounterContext()
{
	public required Player Player { get; set; }
}

internal enum EncounterState
{
	NotStarted,
	InProgress,
	Completed,
}

internal struct EncounterInstance()
{
	public EncounterState State = EncounterState.NotStarted;
	public int WaveIndex;
	public int EnemiesSpawnedThisWave;
	public uint SpawnCooldown;
	public required Encounter Encounter;
}

internal sealed class EnemyEncounters : ModSystem
{
	private sealed class NpcLogic : GlobalNPC
	{
		public override void OnSpawn(NPC npc, IEntitySource source)
		{
			// Reset mapping for this slot.
			enemyToEncounterMapping[npc.whoAmI] = -1;
		}
	}

	public static readonly ConfigEntry<bool> EnableEnemyEncounters = new(ConfigSide.Both, true, "Balance");

	private static readonly List<EncounterInstance> encounters = new();
	/// <summary> Maps spawned enemies to the encounter that spawned them. </summary>
	private static readonly int[] enemyToEncounterMapping = new int[Main.maxNPCs + 1];

	public static int Count => encounters.Count;
	public static ReadOnlySpan<EncounterInstance> Encounters => CollectionsMarshal.AsSpan(encounters);

	static EnemyEncounters()
	{
		Array.Fill(enemyToEncounterMapping, -1);
	}

	public override void Load()
	{
		base.Load();

		MonoModHooks.Modify(typeof(SceneEffectLoader).GetMethod(nameof(SceneEffectLoader.UpdateMusic)), SceneEffectLoaderUpdateMusicInjection);
	}

	public override void PostUpdateWorld()
	{
		UpdateEncounters();
	}

	/// <summary> Creates an encounter with the given parameters. </summary>
	public static void CreateEncounter(in Encounter encounter)
	{
		encounters.Add(new EncounterInstance {
			Encounter = encounter,
			//EndTime = encounter.TimeInSeconds.HasValue ? (ulong)(Main.GameUpdateCount + (encounter.TimeInSeconds.Value * TimeSystem.LogicFramerate)) : null,
		});
	}

	private static void UpdateEncounters()
	{
		if (Main.netMode == NetmodeID.MultiplayerClient) {
			return;
		}

		var span = CollectionsMarshal.AsSpan(encounters);
		StartEncounters(span);
		SpawnEnemies(span);
		ProgressEncounters(span);
		DebugEncounters();
	}

	private static void StartEncounters(Span<EncounterInstance> encounters)
	{
		// Begin encounters approached by a player.
		foreach (ref var instance in encounters) {
			if (instance.State != EncounterState.NotStarted) continue;

			ref readonly var encounter = ref instance.Encounter;
			float sqrRange = encounter.ActivationRange * encounter.ActivationRange;

			foreach (var player in Main.ActivePlayers) {
				if (player.Center.DistanceSQ(encounter.ActivationOrigin) <= sqrRange) {
					var ctx = new EncounterContext {
						Player = player,
					};

					StartEncounter(ref instance, in ctx);
					break;
				}
			}
		}
	}

	private static void StartEncounter(ref EncounterInstance instance, in EncounterContext ctx)
	{
		instance.Encounter.OnActivated?.Invoke(ref instance.Encounter, in ctx);
		instance.State = EncounterState.InProgress;
	}

	private static void SpawnEnemies(Span<EncounterInstance> encounters)
	{
		IEntitySource? source = null;

		for (int encounterIdx = 0; encounterIdx < encounters.Length; encounterIdx++) {
			ref var instance = ref encounters[encounterIdx];
			ref readonly var encounter = ref instance.Encounter;
			ref readonly var wave = ref encounter.Waves[instance.WaveIndex];

			if (instance.State != EncounterState.InProgress) continue;
			if (instance.SpawnCooldown > 0 && --instance.SpawnCooldown > 0) continue;

			// Spawn enemies for the current wave.
			for (int enemyIndexInWave = instance.EnemiesSpawnedThisWave; enemyIndexInWave < wave.Spawns.Length; enemyIndexInWave++) {
				var spawn = wave.Spawns[enemyIndexInWave];

				// Supply overrides for placement.
				if (spawn.SpawnPlacement is { } placement) {
					if (placement.Area == default) placement.Area = encounter.SpawnArea;
					if (placement.AreaOrigin == default) placement.AreaOrigin = encounter.SpawnOrigin;
					spawn.SpawnPlacement = placement;
				}

				source ??= Entity.GetSource_NaturalSpawn();

				if (!EnemySpawning.TrySpawningEnemy(source, in spawn, out var npc)) {
					break;
				}

				instance.EnemiesSpawnedThisWave++;
				instance.SpawnCooldown += spawn.CooldownInTicks;
				enemyToEncounterMapping[npc.whoAmI] = encounterIdx;

				if (instance.SpawnCooldown > 0) {
					break;
				}
			}
		}
	}

	private static void ProgressEncounters(Span<EncounterInstance> encounters)
	{
		for (int encounterIdx = 0; encounterIdx < encounters.Length; encounterIdx++) {
			ref var instance = ref encounters[encounterIdx];
			ref readonly var encounter = ref instance.Encounter;
			ref readonly var wave = ref encounter.Waves[instance.WaveIndex];

			if (instance.State != EncounterState.InProgress) continue;

			// Switch to next wave or end the encounter if all of its spawned enemies have been killed.
			if (instance.EnemiesSpawnedThisWave >= wave.Spawns.Length) {
				int livingCount = CountLivingEnemiesFromEncounter(encounterIdx);

				if (livingCount == 0) {
					if (instance.WaveIndex + 1 >= encounter.Waves.Length) {
						EndEncounter(ref instance);
					} else {
						NextWave(ref instance);
					}
				}
			}
		}
	}

	private static void NextWave(ref EncounterInstance instance)
	{
		instance.WaveIndex++;
		instance.EnemiesSpawnedThisWave = 0;
	}

	private static void EndEncounter(ref EncounterInstance instance)
	{
		instance.State = EncounterState.Completed;
	}

	private static int CountLivingEnemiesFromEncounter(int encounterIndex)
	{
		int result = 0;

		foreach (var npc in Main.ActiveNPCs) {
			if (enemyToEncounterMapping[npc.whoAmI] == encounterIndex) {
				result += 1;
			}
		}

		return result;
	}

	private static void SceneEffectLoaderUpdateMusicInjection(ILContext ctx)
	{
		var il = new ILCursor(ctx);

		// Go to the very bottom, just before 'ret'.
		il.Index = il.Instrs.Count - 1;

		ILUtils.HijackIncomingLabels(il);
		il.EmitLdarg(1);
		il.EmitLdarg(2);
		il.EmitDelegate(UpdateMusic);

		MonoModHooks.DumpIL(OverhaulMod.Instance, ctx);
	}

	private static void UpdateMusic(ref int music, ref SceneEffectPriority priority)
	{
		var localPlayer = Main.LocalPlayer;

		foreach (ref readonly var instance in Encounters) {
			if (instance.State != EncounterState.InProgress) continue;
			if (instance.Encounter.MusicIndex <= 0) continue;
			if (instance.Encounter.SceneEffectPriority < priority) continue;
			
			float increasedSqrRange = MathF.Pow(instance.Encounter.ActivationRange * 3f, 2f);
			if (localPlayer.DistanceSQ(instance.Encounter.ActivationOrigin) > increasedSqrRange) continue;

			priority = instance.Encounter.SceneEffectPriority;
			music = instance.Encounter.MusicIndex;
		}
	}

	[Conditional("DEBUG")]
	private static void DebugEncounters()
	{
		float minDistance = encounters.Count != 0 ? encounters.Min(e => e.Encounter.ActivationOrigin.Distance(Main.LocalPlayer.Center)) : 1f;
		Console.WriteLine($"Encounter distance: {minDistance}");

		if (Main.keyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.NumPad2)) {
			encounters.Clear();
		}
	}
}

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using TerrariaOverhaul.Api.Camera;
using TerrariaOverhaul.Core.Debugging;
using TerrariaOverhaul.Core.Networking;
using TerrariaOverhaul.Utilities.Terraria;
using TerrariaOverhaul.Utilities.Xna;

namespace TerrariaOverhaul.Common.Encounters;

internal enum EnemySpawnEffect
{
	None,
	Teleport,
}

/// <summary> An enemy spawn description. </summary>
internal struct EnemySpawn()
{
	/// <summary> The NPC type to spawn. </summary>
	public required int NpcType { get; set; }
	/// <summary> Pre-determined spawn position to use. Mutually exclusive with <see cref="SpawnPlacement"/>. </summary>
	public required Vector2? SpawnPosition { get; set; }
	/// <summary> Pre-determined spawn position to use. Mutually exclusive with <see cref="SpawnPlacement"/>. </summary>
	public required SpawnPlacement? SpawnPlacement { get; set; }
	/// <summary> The time in ticks that must pass before the next enemy in queue will be spawned. </summary>
	public uint CooldownInTicks { get; set; } = 10;
	/// <summary> Which effect to use for this spawn. </summary>
	public EnemySpawnEffect Effect { get; set; }
}

/// <summary> A description used to calculate a placement for an enemy spawn. </summary>
internal struct SpawnPlacement()
{
	/// <summary> The enemy's width and height. </summary>
	public required Point16 CollisionSize { get; set; }
	/// <summary> The available spawn area to use, in tile-space. </summary>
	public required Rectangle Area { get; set; }
	/// <summary> If provided, a pathfinding check will be performed towards this tile-space position to determine if a given spawn position is valid. </summary>
	public Point16? AreaOrigin { get; set; }
	/// <summary> Whether this enemy has to spawn on the ground. </summary>
	public bool OnGround { get; set; }
	/// <summary> If not zero, the spawn point must not intersect with any of the liquids within this mask. </summary>
	public LiquidMask SkippedLiquids { get; set; } = LiquidMask.All;
	/// <summary> If not zero, the spawn point must be submerged into any of the liquids within this mask. </summary>
	public LiquidMask RequiredLiquids { get; set; }
	/// <summary> How far away from players, in pixels, must the spawn be placed. </summary>
	public float MinDistanceFromPlayers { get; set; } = 256f;
	/// <summary> How far away from existing enemies, in pixels, must the spawn be placed. </summary>
	public float MinDistanceFromEnemies { get; set; } = 256f;
}

/// <summary> Functions for spawning enemies in automated and fancy ways. </summary>
internal static class EnemySpawning
{
	private sealed class EnemySpawnPacket : NetPacket
	{
		public EnemySpawnPacket(NPC npc, EnemySpawnEffect effect, Vector2 position)
		{
			Writer.Write((byte)npc.whoAmI);
			Writer.Write((byte)effect);
			Writer.WriteVector2(position);
		}

		public override void Read(BinaryReader reader, int sender)
		{
			byte npcIndex = reader.ReadByte();
			var effect = (EnemySpawnEffect)reader.ReadByte();
			var position = reader.ReadVector2();

			if (npcIndex < Main.maxNPCs && Main.npc[npcIndex] is { active: true } npc && Main.netMode == NetmodeID.MultiplayerClient) {
				SpawnEffects(npc, effect, position);
			}
		}
	}

	/// <summary>
	/// Attempts to spawn an enemy in the given area, with the given parameters.
	/// <br/> Can fail if spawn logic gives up on picking a viable position.
	/// <br/> Will fail if there are too many enemies in the world.
	/// </summary>
	public static bool TrySpawningEnemy(IEntitySource source, in EnemySpawn spawn, [NotNullWhen(true)] out NPC? npc)
	{
		if (Main.netMode == NetmodeID.MultiplayerClient) {
			throw new InvalidOperationException("Attempted to spawn NPC on multiplayer client.");
		}

		if (spawn.SpawnPosition.HasValue == spawn.SpawnPlacement.HasValue) {
			throw new InvalidOperationException($"A spawn description must have either {spawn.SpawnPlacement} or {spawn.SpawnPosition} specified, not both nor neither.");
		}

		if (spawn.SpawnPosition is not { } spawnPosition) {
			if (!TryFindSpawnPosition(spawn.SpawnPlacement!.Value, out spawnPosition)) {
				npc = default;
				return false;
			}
		}

		npc = NPC.NewNPCDirect(source, spawnPosition, spawn.NpcType);

		if (npc.whoAmI == Main.maxNPCs) {
			return false;
		}

		SpawnEffects(npc, spawn.Effect, spawnPosition);

		return true;
	}

	private static void SpawnEffects(NPC npc, EnemySpawnEffect effect, Vector2 position)
	{
		if (Main.netMode == NetmodeID.Server) {
			MultiplayerSystem.SendPacket(new EnemySpawnPacket(npc, effect, position));
			return;
		}

		if (effect == EnemySpawnEffect.Teleport) {
			SoundEngine.PlaySound(new SoundStyle($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Encounters/EnemySpawn", 2) {
				Volume = 0.33f,
				PitchVariance = 0.2f,
				MaxInstances = 3,
			}, position);

			for (int i = 0; i < 10; i++) {
				Dust.NewDustDirect(npc.position, npc.width, npc.height, DustID.WitherLightning);
			}
		}

		CameraCurios.Create(new CameraCurio {
			Identifier = $"SpawnedEnemy_{npc.whoAmI}",
			Position = npc.Center,
			LengthInSeconds = 2.5f,
			FadeInLength = 0.5f,
			FadeOutLength = 0.5f,
			Weight = 0.05f,
			Callback = new NpcTracker(npc).Center,
		});
	}

	private static bool TryFindSpawnPosition(in SpawnPlacement spawn, out Vector2 spawnPosition)
	{
		if (spawn.Area == default) {
			throw new ArgumentException("Invalid area.");
		}

		const int MaxRandomAttempts = 50;

		float minSqrDistanceFromPlayers = spawn.MinDistanceFromPlayers * spawn.MinDistanceFromPlayers;
		float minSqrDistanceFromEnemies = spawn.MinDistanceFromEnemies * spawn.MinDistanceFromEnemies;

		var sizeInTiles = new Vector2Int(
			(int)MathF.Ceiling(spawn.CollisionSize.X / (float)WorldUtils.TileSizeInPixels),
			(int)MathF.Ceiling(spawn.CollisionSize.Y / (float)WorldUtils.TileSizeInPixels)
		);
		var sizeInTilesHalf = sizeInTiles * 0.5f;
		(int sizeOffsetX1, int sizeOffsetX2) = ((int)MathF.Floor(-sizeInTilesHalf.X), (int)MathF.Floor(sizeInTilesHalf.X) - 1);
		(int sizeOffsetY1, int sizeOffsetY2) = ((int)MathF.Floor(-sizeInTilesHalf.Y), (int)MathF.Floor(sizeInTilesHalf.Y) - 1);

		(int xMin, int xMax) = (Math.Max(0, spawn.Area.X), Math.Min(spawn.Area.X + spawn.Area.Width, Main.maxTilesX - 1));
		(int yMin, int yMax) = (Math.Max(0, spawn.Area.Y), Math.Min(spawn.Area.Y + spawn.Area.Height, Main.maxTilesY - 1));

		// Ensure that the spawn origin is in a location reachable by inflated floodfill.
		Vector2Int adjustedSpawnOrigin = default;
		if (spawn.AreaOrigin is { } spawnOrigin && !WorldUtils.TryFitRectangleIntoTilemap(spawnOrigin, sizeInTiles, out adjustedSpawnOrigin)) {
			spawnPosition = default;
			DebugSystem.DrawRectangle(new Rectangle(spawnOrigin.X * 16, spawnOrigin.Y * 16, 16, 16), Color.Cyan, 8);
			return false;
		}

		for (int i = 0; i < MaxRandomAttempts; i++) {
			(int x, int y) = (Main.rand.Next(xMin, xMax + 1), Main.rand.Next(yMin, yMax + 1));

			Tile baseTile = Main.tile[x, y];
			if (baseTile.HasUnactuatedTile && Main.tileSolid[baseTile.TileType]) continue;

			// Move the spawn point to the ground if needed.
			if (spawn.OnGround) {
				for (int yy = y; yy < Main.maxTilesY + 1; yy++) {
					var tile = Main.tile[x, yy];
					if (tile.HasUnactuatedTile && Main.tileSolid[tile.TileType]) {
						y = yy - 1;
						break;
					}
				}
			}

			// Ensure that the ground position has enough space.
			// While doing this, we also offset the spawn position off the ground, so that the later
			// floodfill cycle does not think that we are in a wall due to collision check inflation.
			if (WorldUtils.TryFitRectangleIntoTilemap(new Vector2Int(x, y), sizeInTiles, out var adjustedSelfRect)) {
				x = adjustedSelfRect.X;
				y = adjustedSelfRect.Y;
			} else {
				continue;
			}

			spawnPosition = new Point(x, y).ToWorldCoordinates(autoAddY: 16);

			// Ensure that this is not too close to a player.
			foreach (var player in Main.ActivePlayers) {
				if (player.DistanceSQ(spawnPosition) <= minSqrDistanceFromPlayers) {
					goto Continue;
				}
			}

			// Ensure that this is not too close to an existing enemy.
			foreach (var npc in Main.ActiveNPCs) {
				if (npc.DistanceSQ(spawnPosition) <= minSqrDistanceFromEnemies) {
					goto Continue;
				}
			}

			// Check for liquids if needed.
			if ((spawn.SkippedLiquids | spawn.RequiredLiquids) != 0) {
				if (!LiquidUtils.CheckAreaWithMasks(new Rectangle(x, y, sizeInTiles.X, sizeInTiles.Y), spawn.SkippedLiquids, spawn.RequiredLiquids)) {
					continue;
				}
			}

			// As the last check, perform a floodfill loop to see if we can reach the encounter's center.
			const int FloodFillExtent = 8;
			var floodFillStart = new Vector2Int(x, y);
			var floodFillArea = new Vector4Int(
				Math.Max(xMin - FloodFillExtent, 1),
				Math.Max(yMin - FloodFillExtent, 1),
				Math.Min(xMax + FloodFillExtent, Main.maxTilesX - 2),
				Math.Min(yMax + FloodFillExtent, Main.maxTilesY - 2)
			);
			var floodFillRect = new Rectangle(
				floodFillArea.X,
				floodFillArea.Y,
				floodFillArea.Z - floodFillArea.X,
				floodFillArea.W - floodFillArea.Y
			);
			bool floodFillSuccess = false;

			foreach (var step in new GeometryUtils.FloodFill(floodFillStart, floodFillRect)) {
				// Inclusive.
				(int checkX1, int checkX2) = (step.Point.X + sizeOffsetX1, step.Point.X + sizeOffsetX2);
				(int checkY1, int checkY2) = (step.Point.Y + sizeOffsetY1, step.Point.Y + sizeOffsetY2);
				bool isPointFree = true;

				for (int checkX = checkX1; checkX <= checkX2; checkX++) {
					for (int checkY = checkY1; checkY <= checkY2; checkY++) {
						var tile = Main.tile[checkX, checkY];
						if (tile.HasUnactuatedTile && Main.tileSolid[tile.TileType]) {
							isPointFree = false;
							goto CycleBreak;
						}
					}
				}

				CycleBreak: step.IsPointFree = isPointFree;

				if (step.Point.X == adjustedSpawnOrigin.X & step.Point.Y == adjustedSpawnOrigin.Y) {
					floodFillSuccess = true;
					break;
				}
			}

			// Failure, the origin point has never been reached.
			if (!floodFillSuccess) {
				continue;
			}

			// Success, all the checks have passed.
			return true;

			// Failure, some deep loop has given up.
			Continue:;
		}

		spawnPosition = default;
		return false;
	}
}

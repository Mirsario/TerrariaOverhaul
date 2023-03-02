using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Core.Configuration;
using TerrariaOverhaul.Core.SimpleEntities;
using TerrariaOverhaul.Core.Tiles;
using TerrariaOverhaul.Utilities;
using On_WorldGen = On.Terraria.WorldGen;

namespace TerrariaOverhaul.Common.TreeFalling;

[Autoload(Side = ModSide.Client)]
public sealed class TreeFallingSystem : ModSystem
{
	public static readonly ConfigEntry<bool> EnableTreeFallingAnimations = new(ConfigSide.Both, "Aesthetics", nameof(EnableTreeFallingAnimations), () => true);

	private static bool isTreeBeingDestroyed;

	public override void Load()
	{
		On_WorldGen.KillTile += KillTileInjection;
	}

	private static void OnTreeDestroyed(Tile tile, Vector2Int tilePosition)
	{
		// Ensure that this is a middle of a tree
		var bottomPosition = tilePosition + Vector2Int.UnitY;
		var bottomTile = Main.tile[bottomPosition.X, bottomPosition.Y];

		if (!bottomTile.HasUnactuatedTile || !(IsTreeTile(bottomTile) || Main.tileSolid[bottomTile.TileType])) {
			return;
		}

		// Get all adjacent tree tiles above this one
		var adjacentTreeParts = GetAdjacentTreeParts(tilePosition);
		var adjacentTreePartsSpan = CollectionsMarshal.AsSpan(adjacentTreeParts);

		// Calculate tilespace AABB
		CalculateTilesAabb(adjacentTreePartsSpan, out var aabbMin, out var aabbMax);

		int treeHeight = aabbMax.Y - aabbMin.Y + 1;
		var aabbMinOffset = new Vector2Int(2, 4);
		var aabbMaxOffset = new Vector2Int(2, 1);

		aabbMin -= aabbMinOffset;
		aabbMax += aabbMaxOffset;

		// Create tree texture
		var sizeInTiles = aabbMax - aabbMin + Vector2Int.One;
		var snapshotTexture = TileSnapshotSystem.CreateSpecificTilesSnapshot(sizeInTiles, aabbMin, adjacentTreePartsSpan);

		// Create tree entity
		SimpleEntity.Instantiate<FallingTreeEntity>(e => {
			e.Position = (tilePosition + new Vector2(0.5f, aabbMaxOffset.Y)) * TileUtils.TileSizeInPixels;
			e.TreeHeight = treeHeight;

			e.IsTextureDisposable = true;
			e.Texture = snapshotTexture;
			e.TextureOrigin = new Vector2(
				(tilePosition.X - aabbMin.X + 0.5f) * TileUtils.TileSizeInPixels,
				snapshotTexture.Height - (aabbMaxOffset.Y * TileUtils.TileSizeInPixels)
			);
		});
	}

	private static List<Vector2Int> GetAdjacentTreeParts(Vector2Int basePosition)
	{
		var tilePositions = new List<Vector2Int>();

		static bool TileCheck(Tile tile)
			=> tile.HasUnactuatedTile && IsTreeTile(tile);

		for (var checkPosition = basePosition; checkPosition.Y >= 0; checkPosition.Y--) {
			var checkTile = Main.tile[checkPosition.X, checkPosition.Y];

			if (!TileCheck(checkTile)) {
				break;
			}

			tilePositions.Add(checkPosition);

			for (int xOffset = -1; xOffset <= 1; xOffset += 2) {
				var sidePosition = checkPosition + new Vector2Int(xOffset, 0);

				if (Main.tile.TryGet(sidePosition, out Tile sideTile) && TileCheck(sideTile)) {
					tilePositions.Add(sidePosition);
				}
			}
		}

		return tilePositions;
	}

	private static void CalculateTilesAabb(ReadOnlySpan<Vector2Int> tilePositions, out Vector2Int aabbMin, out Vector2Int aabbMax)
	{
		aabbMin = tilePositions[0];
		aabbMax = tilePositions[0];

		for (int i = 1; i < tilePositions.Length; i++) {
			var thisPosition = tilePositions[i];

			aabbMin = Vector2Int.Min(aabbMin, thisPosition);
			aabbMax = Vector2Int.Max(aabbMax, thisPosition);
		}
	}

	private static bool IsTreeTile(Tile tile)
		=> TileID.Sets.IsATreeTrunk[tile.TileType];

	private static void KillTileInjection(On_WorldGen.orig_KillTile original, int x, int y, bool fail, bool effectOnly, bool noItem)
	{
		var tile = Main.tile[x, y];
		var tilePosition = new Vector2Int(x, y);
		bool wasTreeBeingDestroyed = isTreeBeingDestroyed;

		if (!Main.dedServ && !fail && EnableTreeFallingAnimations && IsTreeTile(tile) && !isTreeBeingDestroyed) {
			OnTreeDestroyed(tile, tilePosition);

			isTreeBeingDestroyed = true;
		}

		original(x, y, fail, effectOnly, noItem);

		isTreeBeingDestroyed = wasTreeBeingDestroyed;
	}
}

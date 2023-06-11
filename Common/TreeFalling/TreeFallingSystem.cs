using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Core.Configuration;
using TerrariaOverhaul.Core.EntityCapturing;
using TerrariaOverhaul.Core.SimpleEntities;
using TerrariaOverhaul.Core.Tiles;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.TreeFalling;

[Autoload(Side = ModSide.Client)]
public sealed class TreeFallingSystem : ModSystem
{
	private struct TreeCreationData
	{
		public int TreeHeight;
		public bool DestroyBottomTile;
		public Direction1D FallDirection;
		public Vector2Int BasePosition;
		public Vector2Int BottomPosition;
		public Vector2Int AabbMin;
		public Vector2Int AabbMax;
		public Vector2Int TextureAabbMin;
		public Vector2Int TextureAabbMax;
		public Vector2Int TextureAabbMinOffset;
		public Vector2Int TextureAabbMaxOffset;
		public RenderTarget2D Texture;
		public List<Vector2Int> TreeTilePositions;
		public List<ItemCapture>? CapturedItems;
		public List<DustCapture>? CapturedDusts;
	}

	private const int MinimalTreeHeight = 3;

	public static readonly ConfigEntry<bool> EnableTreeFallingAnimations = new(ConfigSide.Both, "Aesthetics", nameof(EnableTreeFallingAnimations), () => true);
	public static readonly ConfigEntry<bool> DestroyStumpsAfterTreeFalls = new(ConfigSide.Both, "Aesthetics", nameof(DestroyStumpsAfterTreeFalls), () => true);

	private static bool isTreeBeingDestroyed;
	private static bool isTreeHitRedirectedFromStump;
	private static Player? playerCurrentlyUsingTools;

	public override void Load()
	{
		On_WorldGen.KillTile += KillTileDetour;
		On_Player.ItemCheck_UseMiningTools_ActuallyUseMiningTool += ActuallyUseMiningToolDetour;
	}

	private static bool PrepareForTreeCreation(Vector2Int basePosition, [NotNullWhen(true)] out TreeCreationData data)
	{
		var baseTile = Main.tile[basePosition.X, basePosition.Y];

		data.BasePosition = basePosition;

		if (!IsATreeTile(baseTile)) {
			data = default;
			return false;
		}

		// Ensure that this is a middle of a tree
		data.BottomPosition = basePosition + Vector2Int.UnitY;

		var bottomTile = Main.tile[data.BottomPosition.X, data.BottomPosition.Y];

		if (!bottomTile.HasUnactuatedTile || !(IsATreeTile(bottomTile) || Main.tileSolid[bottomTile.TileType])) {
			data = default;
			return false;
		}

		// Get all adjacent tree tiles above this one
		data.TreeTilePositions = GetAdjacentTreeParts(basePosition);

		var treeTilePositionsSpan = CollectionsMarshal.AsSpan(data.TreeTilePositions);

		// Calculate tilespace AABBs
		CalculateTilesAabb(treeTilePositionsSpan, out data.AabbMin, out data.AabbMax);

		data.TextureAabbMinOffset = new Vector2Int(2, 4);
		data.TextureAabbMaxOffset = new Vector2Int(2, 1);

		data.TextureAabbMin = data.AabbMin - data.TextureAabbMinOffset;
		data.TextureAabbMax = data.AabbMax + data.TextureAabbMaxOffset;

		// Check tree height
		data.TreeHeight = data.AabbMax.Y - data.AabbMin.Y + 1;

		if (data.TreeHeight < MinimalTreeHeight) {
			data = default;
			return false;
		}

		// Get fall direction
		data.FallDirection = GetTreeFallDirection(basePosition);

		// Set if the bottom tile should be removed after the tree falls
		data.DestroyBottomTile = IsATreeRoot(data.BottomPosition) && isTreeHitRedirectedFromStump && DestroyStumpsAfterTreeFalls;

		// Create tree texture
		var sizeInTiles = data.TextureAabbMax - data.TextureAabbMin + Vector2Int.One;

		data.Texture = TileSnapshotSystem.CreateSpecificTilesSnapshot(sizeInTiles, data.TextureAabbMin, treeTilePositionsSpan);

		// Assign defaults to remaining fields
		data.CapturedItems = default;
		data.CapturedDusts = default;

		return true;
	}

	private static Direction1D GetTreeFallDirection(Vector2Int basePosition)
	{
		if (playerCurrentlyUsingTools is Player toolingPlayer) {
			return toolingPlayer.direction >= 0 ? Direction1D.Right : Direction1D.Left;
		}

		return basePosition.X % 2 == 0 ? Direction1D.Right : Direction1D.Left;
	}

	private static void CreateFallingTree(TreeCreationData data)
	{
		// Create tree entity
		SimpleEntity.Instantiate<FallingTreeEntity>(e => {
			e.TreeHeight = data.TreeHeight;
			e.Position = (data.BasePosition + new Vector2(0.5f, data.TextureAabbMaxOffset.Y)) * TileUtils.TileSizeInPixels;
			e.BottomTilePosition = data.BottomPosition;
			e.DestroyBottomTile = data.DestroyBottomTile;
			e.FallDirection = data.FallDirection;

			e.Texture = data.Texture;
			e.TextureOrigin = new Vector2(
				(data.BasePosition.X - data.TextureAabbMin.X + 0.5f) * TileUtils.TileSizeInPixels,
				data.Texture.Height - (data.TextureAabbMaxOffset.Y * TileUtils.TileSizeInPixels)
			);
			e.IsTextureDisposable = true;

			e.CapturedItems = data.CapturedItems;
			e.CapturedDusts = data.CapturedDusts;
		});
	}

	private static List<Vector2Int> GetAdjacentTreeParts(Vector2Int basePosition)
	{
		var tilePositions = new List<Vector2Int>();

		static bool TileCheck(Tile tile)
			=> tile.HasUnactuatedTile && IsATreeTile(tile);

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

	private static bool IsATreeTile(Tile tile)
		=> TileID.Sets.IsATreeTrunk[tile.TileType];

	private static bool IsATreeRoot(Vector2Int position)
		=> IsATreeTile(Main.tile.Get(position)) && (!Main.tile.TryGet(position + Vector2Int.UnitY, out var lowerTile) || !IsATreeTile(lowerTile));

	private static bool IsATreeStump(Vector2Int position, bool requireSpace = false)
	{
		if (!IsATreeRoot(position)) {
			return false;
		}

		if (!Main.tile.TryGet(position - Vector2Int.UnitY, out var upperTile) && requireSpace) {
			return false;
		}

		if (IsATreeTile(upperTile)) {
			return false;
		}

		return true;
	}

	private static void ActuallyUseMiningToolDetour(On_Player.orig_ItemCheck_UseMiningTools_ActuallyUseMiningTool original, Player player, Item selectedItem, out bool canHitWalls, int x, int y)
	{
		var tilePosition = new Vector2Int(x, y);

		// Redirect damage to the top if a non-stump tree root is hit.
		if (!isTreeHitRedirectedFromStump && !isTreeBeingDestroyed && IsATreeRoot(tilePosition) && !IsATreeStump(tilePosition) && tilePosition.Y > 0) {
			try {
				isTreeHitRedirectedFromStump = true;

				ActuallyUseMiningToolDetour(original, player, selectedItem, out canHitWalls, x, y - 1);
			}
			finally {
				isTreeHitRedirectedFromStump = false;
			}

			return;
		}

		// Save the player currently executing this method to easily detect the direction that trees should fall towards.
		var playerPreviouslyUsingTools = playerCurrentlyUsingTools;

		try {
			playerCurrentlyUsingTools = player;

			original(player, selectedItem, out canHitWalls, x, y);
		}
		finally {
			playerCurrentlyUsingTools = playerPreviouslyUsingTools;
		}
	}

	private static void KillTileDetour(On_WorldGen.orig_KillTile original, int x, int y, bool fail, bool effectOnly, bool noItem)
	{
		var tilePosition = new Vector2Int(x, y);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		void Original()
			=> original(x, y, fail, effectOnly, noItem);

		if (Main.dedServ || fail || WorldGen.gen || WorldGen.generatingWorld || !EnableTreeFallingAnimations || !Program.IsMainThread) {
			Original();
			return;
		}

		if (isTreeBeingDestroyed) {
			// Suspend captures for non-tree tiles being broken
			bool suspendCaptures = !IsATreeTile(Main.tile[x, y]);

			using (suspendCaptures ? ItemCapturing.Suspend() : default)
			using (suspendCaptures ? DustCapturing.Suspend() : default) {
				Original();
				return;
			}
		}

		if (!PrepareForTreeCreation(tilePosition, out var data)) {
			Original();
			return;
		}

		using (ItemCapturing.Capture(out data.CapturedItems))
		using (DustCapturing.Capture(out data.CapturedDusts)) {
			try {
				isTreeBeingDestroyed = true;

				Original();
			}
			finally {
				isTreeBeingDestroyed = false;
			}
		}

		CreateFallingTree(data);
	}
}

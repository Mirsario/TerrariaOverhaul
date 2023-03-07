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
using On_WorldGen = On.Terraria.WorldGen;

namespace TerrariaOverhaul.Common.TreeFalling;

[Autoload(Side = ModSide.Client)]
public sealed class TreeFallingSystem : ModSystem
{
	private struct TreeCreationData
	{
		public int TreeHeight;
		public bool DestroyStump;
		public Vector2Int BasePosition;
		public Vector2Int BottomPosition;
		public Vector2Int AabbMin;
		public Vector2Int AabbMax;
		public Vector2Int TextureAabbMin;
		public Vector2Int TextureAabbMax;
		public Vector2Int TextureAabbMinOffset;
		public Vector2Int TextureAabbMaxOffset;
		public List<Vector2Int> TreeTilePositions;
		public RenderTarget2D Texture;
		public List<ItemCapture>? CapturedItems;
		public List<DustCapture>? CapturedDusts;
	}

	private const int MinimalTreeHeight = 3;

	public static readonly ConfigEntry<bool> EnableTreeFallingAnimations = new(ConfigSide.Both, "Aesthetics", nameof(EnableTreeFallingAnimations), () => true);
	public static readonly ConfigEntry<bool> DestroyStumpsAfterTreeFalls = new(ConfigSide.Both, "Aesthetics", nameof(DestroyStumpsAfterTreeFalls), () => true);

	private static bool isTreeBeingDestroyed;
	private static bool isTreeHitRedirectedFromStump;

	public override void Load()
	{
		On_WorldGen.KillTile += KillTileInjection;
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

		data.DestroyStump = IsATreeRoot(data.BottomPosition) && isTreeHitRedirectedFromStump && DestroyStumpsAfterTreeFalls;

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

		// Create tree texture
		var sizeInTiles = data.TextureAabbMax - data.TextureAabbMin + Vector2Int.One;

		data.Texture = TileSnapshotSystem.CreateSpecificTilesSnapshot(sizeInTiles, data.TextureAabbMin, treeTilePositionsSpan);

		// Assign defaults to remaining fields
		data.CapturedItems = default;
		data.CapturedDusts = default;

		return true;
	}

	private static void CreateFallingTree(in TreeCreationData data)
	{
		// Create tree entity
		int treeHeight = data.TreeHeight;
		var entityPosition = (data.BasePosition + new Vector2(0.5f, data.TextureAabbMaxOffset.Y)) * TileUtils.TileSizeInPixels;
		var bottomPosition = data.BottomPosition;
		bool destroyBottomTile = data.DestroyStump;

		var texture = data.Texture;
		var textureOrigin = new Vector2(
			(data.BasePosition.X - data.TextureAabbMin.X + 0.5f) * TileUtils.TileSizeInPixels,
			texture.Height - (data.TextureAabbMaxOffset.Y * TileUtils.TileSizeInPixels)
		);

		var capturedLoot = data.CapturedItems;
		var capturedDusts = data.CapturedDusts;

		// Create tree entity
		SimpleEntity.Instantiate<FallingTreeEntity>(e => {
			e.TreeHeight = treeHeight;
			e.Position = entityPosition;
			e.BottomTilePosition = bottomPosition;
			e.DestroyBottomTile = destroyBottomTile;

			e.Texture = texture;
			e.TextureOrigin = textureOrigin;
			e.IsTextureDisposable = true;

			e.CapturedItems = capturedLoot;
			e.CapturedDusts = capturedDusts;
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

	private static void KillTileInjection(On_WorldGen.orig_KillTile original, int x, int y, bool fail, bool effectOnly, bool noItem)
	{
		var tilePosition = new Vector2Int(x, y);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		void Original()
			=> original(x, y, fail, effectOnly, noItem);

		if (Main.dedServ || WorldGen.gen || WorldGen.generatingWorld || !EnableTreeFallingAnimations || !Program.IsMainThread) {
			Original();
			return;
		}

		// Redirect damage to the top if a stump is hit.
		if (IsATreeRoot(tilePosition) && !IsATreeStump(tilePosition) && tilePosition.Y > 0 && !isTreeBeingDestroyed) {
			try {
				isTreeHitRedirectedFromStump = true;

				WorldGen.KillTile(tilePosition.X, tilePosition.Y - 1, fail, effectOnly, noItem);
			}
			finally {
				isTreeHitRedirectedFromStump = false;
			}

			return;
		}

		if (fail) {
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

		CreateFallingTree(in data);
	}
}

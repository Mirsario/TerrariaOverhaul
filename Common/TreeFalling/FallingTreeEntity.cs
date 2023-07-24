using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using TerrariaOverhaul.Common.Items;
using TerrariaOverhaul.Core.EntityCapturing;
using TerrariaOverhaul.Core.SimpleEntities;
using TerrariaOverhaul.Core.Time;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.TreeFalling;

public sealed class FallingTreeEntity : SimpleEntity
{
	public static readonly SoundStyle TreeFallingSound = new($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Aesthetics/Trees/TreeFalling", 2) {
		Volume = 0.5f,
		PitchVariance = 0.2f,
	};
	public static readonly SoundStyle TreeGroundHitSound = new($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Aesthetics/Trees/TreeGroundHit", 2) {
		Volume = 0.5f,
		PitchVariance = 0.2f,
	};

	public int TreeHeight;
	public float Rotation;
	public Direction1D FallDirection = Direction1D.Right;
	public Vector2 Gravity = Vector2.UnitY * 30f * TileUtils.TileSizeInPixels;
	public Vector2 Position;
	public Vector2 Velocity;
	public Vector2 TextureOrigin;
	public RenderTarget2D? Texture;
	public bool IsTextureDisposable = true;
	// 
	public List<ItemCapture>? CapturedItems;
	public List<DustCapture>? CapturedDusts;
	//
	public Vector2Int BottomTilePosition;
	public bool ShouldDestroyBottomTile;
	public (Entity Entity, Item? Item)? HitterInfo;

	public override void Init()
	{
		if (Main.dedServ) {
			return;
		}

		// Lame initialization design.
		if (Texture == null) {
			throw new InvalidOperationException($"{nameof(Texture)} property must be assigned during pre-initialization.");
		}

		if (!Main.dedServ) {
			SoundEngine.PlaySound(in TreeFallingSound, Position);
		}
	}

	public override void Update()
	{
		if (TileCollisionCheck()) {
			Destroy(allowEffects: true);
			return;
		}

		const float AngleToDetachAt = 75f;

		if (MathF.Abs(Rotation) >= MathHelper.ToRadians(AngleToDetachAt) || !CheckBottomTileIntegrity(out _)) {
			Velocity += Gravity * TimeSystem.LogicDeltaTime;
		}

		Position += Velocity * TimeSystem.LogicDeltaTime;

		const float MinRotationSpeed = 2.5f;
		const float MaxRotationSpeed = 110.0f;

		float rotationSpeed = MathHelper.Lerp(MinRotationSpeed, MaxRotationSpeed, MathUtils.Clamp01(MathF.Abs(Rotation / MathHelper.PiOver2)));

		Rotation += MathHelper.ToRadians(rotationSpeed * (float)FallDirection * TimeSystem.LogicDeltaTime);
	}

	public override void Draw(SpriteBatch sb)
	{
		if (Texture?.IsDisposed != false) {
			return;
		}

		var color = Lighting.GetColor(Position.ToTileCoordinates());

		sb.Draw(Texture, Position - Main.screenPosition, null, color, Rotation, TextureOrigin, 1.0f, SpriteEffects.None, 0f);
	}

	protected override void OnDestroyed(bool allowEffects)
	{
		InstantiateItems();

		if (allowEffects && !Main.dedServ) {
			InstantiateDusts();

			var soundPosition = Position + new Vector2(0f, -TreeHeight * TileUtils.TileSizeInPixels * 0.5f).RotatedBy(Rotation);

			SoundEngine.PlaySound(in TreeGroundHitSound, soundPosition);
		}

		TryDestroyingBottomTile();

		if (IsTextureDisposable && Texture is { IsDisposed: false }) {
			Texture.Dispose();
			Texture = null;
		}
	}

	private bool TileCollisionCheck()
	{
		var position = Position;
		float rotation = Rotation;

		for (int i = 0; i < TreeHeight; i++) {
			var positionOffset = new Vector2(0f, -i * TileUtils.TileSizeInPixels);
			var transformedOffset = positionOffset.RotatedBy(rotation);

			var offsetPosition = position + transformedOffset;
			var tilePosition = offsetPosition.ToTileCoordinates();

			if (!Main.tile.TryGet(tilePosition, out var tile)) {
				continue;
			}

			if (tile.HasUnactuatedTile && Main.tileSolid[tile.TileType]) {
				return true;
			}
		}

		return false;
	}

	private bool CheckBottomTileIntegrity(out Tile tile)
	{
		if (!Main.tile.TryGet(BottomTilePosition, out tile) || !tile.HasUnactuatedTile) {
			return false;
		}

		if (!TileID.Sets.IsATreeTrunk[tile.TileType]) {
			return false;
		}

		return true;
	}

	private void TryDestroyingBottomTile()
	{
		if (ShouldDestroyBottomTile && CheckBottomTileIntegrity(out Tile bottomTile)) {
			DestroyBottomTile(bottomTile);

			ShouldDestroyBottomTile = false;
		}
	}

	private void DestroyBottomTile(Tile bottomTile)
	{
		if (HitterInfo is { Entity: Player { active: true } player, Item: Item { IsAir: false, axe: > 0 } item } && player.IsLocal()) {
			// Break using a hit of the last used tool, but force the hit to succeed.
			ref bool tileNoFail = ref Main.tileNoFail[bottomTile.TileType];
			ref int tileTargetX = ref Player.tileTargetX;
			ref int tileTargetY = ref Player.tileTargetY;

			bool tileNoFailCopy = tileNoFail;
			int tileTargetXCopy = tileTargetX;
			int tileTargetYCopy = tileTargetY;

			try {
				tileNoFail = true;
				tileTargetX = BottomTilePosition.X;
				tileTargetY = BottomTilePosition.Y;

				PlayerItemUse.UseMiningTool(player, item, out _, BottomTilePosition.X, BottomTilePosition.Y);
			}
			finally {
				tileNoFail = tileNoFailCopy;
				tileTargetX = tileTargetXCopy;
				tileTargetY = tileTargetYCopy;
			}

			return;
		}
		
		// Fallback
		if (Main.netMode != NetmodeID.MultiplayerClient) {
			WorldGen.KillTile(BottomTilePosition.X, BottomTilePosition.Y);
		}
	}

	private void InstantiateItems()
	{
		if (Main.netMode == NetmodeID.MultiplayerClient || CapturedItems == null) {
			return;
		}

		for (int i = 0, count = CapturedItems.Count; i < count; i++) {
			var capture = CapturedItems[i];
			var adjustedPosition = capture.Position.RotatedBy(Rotation, Position);

			Item.NewItem(capture.Source, adjustedPosition, capture.Type, capture.Stack, prefixGiven: capture.Prefix);
		}

		CapturedItems = null;
	}

	private void InstantiateDusts()
	{
		if (Main.dedServ || CapturedDusts == null) {
			return;
		}

		for (int i = 0, count = CapturedDusts.Count; i < count; i++) {
			var capture = CapturedDusts[i];
			var adjustedPosition = capture.Position.RotatedBy(Rotation, Position);

			Dust.NewDust(adjustedPosition, 1, 1, capture.Type, capture.Velocity.X, capture.Velocity.Y, capture.Alpha, capture.NewColor, capture.Scale);
		}

		CapturedDusts = null;
	}
}

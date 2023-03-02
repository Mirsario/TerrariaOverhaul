using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using TerrariaOverhaul.Core.SimpleEntities;
using TerrariaOverhaul.Core.Time;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.TreeFalling;

public sealed class FallingTreeEntity : SimpleEntity
{
	public int TreeHeight;
	public float Rotation;
	public Vector2 Position;
	public Vector2 TextureOrigin;
	public RenderTarget2D? Texture;
	public bool IsTextureDisposable = true;

	public override void Init()
	{
		// Lame design.
		if (Texture == null) {
			throw new InvalidOperationException($"{nameof(Texture)} property must be assigned during pre-initialization.");
		}
	}

	public override void Update()
	{
		if (TileCollisionCheck() || MathF.Abs(Rotation) >= MathHelper.Pi) {
			Destroy(allowEffects: true);
		}

		float rotationSpeed = MathHelper.Lerp(2.5f, 110f, MathUtils.Clamp01(MathF.Abs(Rotation / MathHelper.PiOver2)));

		Rotation += MathHelper.ToRadians(rotationSpeed * TimeSystem.LogicDeltaTime);
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
}

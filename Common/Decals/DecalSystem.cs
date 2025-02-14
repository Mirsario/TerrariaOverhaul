// Copyright (c) 2020-2025 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using TerrariaOverhaul.Core.Chunks;
using TerrariaOverhaul.Core.Configuration;
using TerrariaOverhaul.Core.Debugging;
using TerrariaOverhaul.Utilities.Terraria;
using TerrariaOverhaul.Utilities.Xna;

namespace TerrariaOverhaul.Common.Decals;

public struct DecalInfo
{
	public static Texture2D DefaultTexture => TextureAssets.BlackTile.Value;

	public Texture2D Texture = DefaultTexture;
	public Rectangle? SrcRect;
	public Vector2 Position;
	public Vector2 Scale = Vector2.One;
	public Color Color = Color.White;
	public float Rotation;
	public bool IfChunkExists;

	public Vector2 Size {
		readonly get => Texture.Size() / Scale;
		set => Scale = value / Texture.Size();
	}

	public DecalInfo() { }

	public readonly Vector4 CalculateAabbRectangle()
	{
		var halfSize = Texture.Size() * Scale * 0.5f;

		if (Rotation == 0f) {
			return new Vector4(
				Position.X - halfSize.X, Position.Y - halfSize.Y,
				Position.X + halfSize.X, Position.Y + halfSize.Y
			);
		}

		var xy = new Vector2(-halfSize.X, -halfSize.Y).RotatedBy(Rotation);
		var zy = new Vector2( halfSize.X, -halfSize.Y).RotatedBy(Rotation);
		var newSize = new Vector2(
			MathF.Max(MathF.Abs(xy.X), MathF.Abs(zy.X)),
			MathF.Max(MathF.Abs(xy.Y), MathF.Abs(zy.Y))
		);
		var result = new Vector4(
			Position.X - newSize.X,
			Position.Y - newSize.Y,
			Position.X + newSize.X,
			Position.Y + newSize.Y
		);

		return result;
	}
}

[Autoload(Side = ModSide.Client)]
public sealed class DecalSystem : ModSystem
{
	public static readonly BlendState DefaultBlendState = BlendState.AlphaBlend;
	public static readonly ConfigEntry<bool> EnableDecals = new(ConfigSide.ClientOnly, true, "BloodAndGore");
	
	private static readonly List<DecalStyle> decalStyles = new();

	public static Asset<Effect>? BloodShader { get; private set; }

	public static ReadOnlySpan<DecalStyle> DecalStyles => CollectionsMarshal.AsSpan(decalStyles);

	public override void Load()
	{
		BloodShader = Mod.Assets.Request<Effect>("Assets/Shaders/Blood");

		DecalStyle.RegisterDefaultStyles();
	}

#if DEBUG && false // Decal debugging hotkey.
	public override void PostDrawTiles()
	{
		if (Core.Input.InputSystem.GetKey(Microsoft.Xna.Framework.Input.Keys.K)) {
			DecalSystem.AddDecals(DecalStyle.Opaque, new DecalInfo {
				Position = Main.MouseWorld,
				Texture = Mod.Assets.Request<Texture2D>("Content/Menus/Logo", AssetRequestMode.ImmediateLoad).Value,
				Scale = new Vector2(1f, 1f),
			});
		}
	}
#endif

	public static void RegisterStyle(DecalStyle style)
	{
		if (style.Id != -1) {
			throw new InvalidOperationException($"Tried to register a {nameof(DecalStyle)} that was already registered!");
		}

		style.Id = decalStyles.Count;

		decalStyles.Add(style);
	}

	public static void ClearDecals(Rectangle dst)
		=> AddDecals(DecalStyle.Opaque, new DecalInfo {
			Position = new Vector2(dst.X + dst.Width * 0.5f, dst.Y + dst.Height * 0.5f),
			Size = dst.Size(),
			Color = Color.Transparent,
			IfChunkExists = true,
		});

	public static void ClearDecals(Texture2D texture, Rectangle dst, Color color)
		=> AddDecals(DecalStyle.Subtractive, new DecalInfo {
			Texture = texture,
			Position = new Vector2(dst.X + dst.Width * 0.5f, dst.Y + dst.Height * 0.5f),
			Size = dst.Size(),
			Color = color,
			IfChunkExists = true,
		});

	public static void AddDecals(DecalStyle style, in DecalInfo decal)
	{
		if (Main.dedServ || WorldGen.gen || WorldGen.IsGeneratingHardMode || !EnableDecals) { // || !ConfigSystem.local.Clientside.BloodAndGore.enableTileBlood) {
			return;
		}

		var aabb = decal.CalculateAabbRectangle();
		var rect = new Rectangle((int)aabb.X, (int)aabb.Y, (int)(aabb.Z - aabb.X), (int)(aabb.W - aabb.Y));

		DebugSystem.DrawRectangle(rect, Color.Bisque);

		var chunkStart = new Vector2Int(
			(int)aabb.X / WorldUtils.TileSizeInPixels / Chunk.MaxChunkSize,
			(int)aabb.Y / WorldUtils.TileSizeInPixels / Chunk.MaxChunkSize
		);
		var chunkEnd = new Vector2Int(
			(int)aabb.Z / WorldUtils.TileSizeInPixels / Chunk.MaxChunkSize,
			(int)aabb.W / WorldUtils.TileSizeInPixels / Chunk.MaxChunkSize
		);

		// The provided rectangle will be split between chunks, possibly into multiple draws.
		for (int chunkY = chunkStart.Y; chunkY <= chunkEnd.Y; chunkY++) {
			for (int chunkX = chunkStart.X; chunkX <= chunkEnd.X; chunkX++) {
				var chunkPoint = new Vector2Int(chunkX, chunkY);

				if (!(decal.IfChunkExists ? ChunkSystem.TryGetChunk(chunkPoint, out Chunk chunk) : ChunkSystem.TryGetOrCreateChunk(chunkPoint, out chunk!))) {
					continue;
				}

				chunk.Components.Get<ChunkDecals>().AddDecals(style, in decal);

				// So much unnecessary overengineering below!
				/*
				var localDstRect = (RectFloat)decal.DstRect;

				// Clip the destination rectangle to the chunk's bounds.
				localDstRect = RectFloat.FromPoints(
					Math.Max(localDstRect.x, chunk.WorldRectangle.x),
					Math.Max(localDstRect.y, chunk.WorldRectangle.y),
					Math.Min(localDstRect.Right, chunk.WorldRectangle.Right),
					Math.Min(localDstRect.Bottom, chunk.WorldRectangle.Bottom)
				);

				// Move the destination rectangle to local space.
				localDstRect.x -= chunk.WorldRectangle.x;
				localDstRect.y -= chunk.WorldRectangle.y;
				// Divide the destination rectangle, since decal RTs have halved resolution.
				localDstRect.x /= 2;
				localDstRect.y /= 2;
				localDstRect.width /= 2;
				localDstRect.height /= 2;

				// Clip the source rectangle.
				var destinationRectInChunkSpace = RectFloat.FromPoints(((RectFloat)decal.DstRect).Points / Chunk.MaxChunkSizeInPixels);
				var clippedRectInChunkSpace = RectFloat.FromPoints(
					Math.Max(destinationRectInChunkSpace.Left, chunk.Rectangle.Left),
					Math.Max(destinationRectInChunkSpace.Top, chunk.Rectangle.Top),
					Math.Min(destinationRectInChunkSpace.Right, chunk.Rectangle.Right),
					Math.Min(destinationRectInChunkSpace.Bottom, chunk.Rectangle.Bottom)
				);

				var srcRect = decal.SrcRect ?? decal.Texture.Bounds;
				var localSrcRect = (Rectangle)new RectFloat(
					srcRect.X + (clippedRectInChunkSpace.x - destinationRectInChunkSpace.x) * (chunk.WorldRectangle.width / decal.DstRect.Width) * srcRect.Width,
					srcRect.Y + (clippedRectInChunkSpace.y - destinationRectInChunkSpace.y) * (chunk.WorldRectangle.height / decal.DstRect.Height) * srcRect.Height,
					(clippedRectInChunkSpace.width / destinationRectInChunkSpace.width) * srcRect.Width,
					(clippedRectInChunkSpace.height / destinationRectInChunkSpace.height) * srcRect.Height
				);

				// Enqueue a draw for the chunk component to do on its own.
				var chunkDecal = decal with {
					SrcRect = localSrcRect,
					DstRect = (Rectangle)localDstRect,
				};

				chunk.Components.Get<ChunkDecals>().AddDecals(style, in chunkDecal);
				*/
			}
		}
	}
}

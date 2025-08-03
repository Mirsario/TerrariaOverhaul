// Copyright (c) 2020-2025 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using System;
using System.Buffers;
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
using TerrariaOverhaul.Core.Data;
using TerrariaOverhaul.Core.Debugging;
using TerrariaOverhaul.Core.Lighting;
using TerrariaOverhaul.Utilities;
using TerrariaOverhaul.Utilities.Terraria;
using TerrariaOverhaul.Utilities.Xna;
using BitOperations = System.Numerics.BitOperations;

namespace TerrariaOverhaul.Common.Decals;

public struct DecalStyleData()
{
	public uint NumDecalsToDraw = 0;
	public DecalInfo[] DecalsToDraw = [];
}
public struct ChunkDecals() : IComponent
{
	public RenderTarget2D? Texture;
	public DecalStyleData[] DecalStyleData = [];
}

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
			Position.X - newSize.X, Position.Y - newSize.Y,
			Position.X + newSize.X, Position.Y + newSize.Y
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
		Main.OnPreDraw += OnPreDraw;
		Chunks.OnChunkDestroyed += RemoveChunkComponent;

		DecalStyle.RegisterDefaultStyles();

		ThreadUtils.RunOnMainThread(() => {
			BloodShader = Mod.Assets.Request<Effect>("Assets/Shaders/Blood");
		});
	}
	public override void Unload()
	{
		Main.OnPreDraw -= OnPreDraw;
	}

	private void OnPreDraw(GameTime gameTime)
	{
		AddPendingDecals();
	}
	public override void PostDrawTiles()
	{
		RenderDecalsInWorld();

#if DEBUG && true // Decal debugging hotkey.
		if (Core.Input.InputSystem.GetKey(Microsoft.Xna.Framework.Input.Keys.K)) {
			AddDecals(DecalStyle.Default, new DecalInfo {
				Position = Main.MouseWorld,
				Texture = Mod.Assets.Request<Texture2D>("Content/Menus/Logo", AssetRequestMode.ImmediateLoad).Value,
				Scale = new Vector2(1f, 1f),
			});
		}
#endif
	}

	public static void RegisterStyle(DecalStyle style)
	{
		if (style.Id != -1) {
			throw new InvalidOperationException($"Tried to register a {nameof(DecalStyle)} that was already registered!");
		}

		style.Id = decalStyles.Count;

		decalStyles.Add(style);
	}

	public static void ClearDecals(Rectangle dst)
	{
		AddDecals(DecalStyle.Opaque, new DecalInfo {
			Position = new Vector2(dst.X + dst.Width * 0.5f, dst.Y + dst.Height * 0.5f),
			Size = dst.Size(),
			Color = Color.Transparent,
			IfChunkExists = true,
		});
	}
	public static void ClearDecals(Texture2D texture, Rectangle dst, Color color)
	{
		AddDecals(DecalStyle.Subtractive, new DecalInfo {
			Texture = texture,
			Position = new Vector2(dst.X + dst.Width * 0.5f, dst.Y + dst.Height * 0.5f),
			Size = dst.Size(),
			Color = color,
			IfChunkExists = true,
		});
	}

	public static void AddDecals(DecalStyle style, in DecalInfo decal)
	{
		if (Main.dedServ || WorldGen.gen || WorldGen.IsGeneratingHardMode || !EnableDecals) {
			return;
		}

		var aabb = decal.CalculateAabbRectangle();
		var rect = new Rectangle((int)aabb.X, (int)aabb.Y, (int)(aabb.Z - aabb.X), (int)(aabb.W - aabb.Y));

		DebugSystem.DrawRectangle(rect, Color.Bisque);

		var chunkStart = new Vector2Int(
			(int)aabb.X / WorldUtils.TileSizeInPixels / Chunks.MaxChunkSize,
			(int)aabb.Y / WorldUtils.TileSizeInPixels / Chunks.MaxChunkSize
		);
		var chunkEnd = new Vector2Int(
			(int)aabb.Z / WorldUtils.TileSizeInPixels / Chunks.MaxChunkSize,
			(int)aabb.W / WorldUtils.TileSizeInPixels / Chunks.MaxChunkSize
		);

		// The provided rectangle will be split between chunks, possibly into multiple draws.
		for (int chunkY = chunkStart.Y; chunkY <= chunkEnd.Y; chunkY++) {
			for (int chunkX = chunkStart.X; chunkX <= chunkEnd.X; chunkX++) {
				var chunkPoint = new Vector2Int(chunkX, chunkY);

				if (!(decal.IfChunkExists ? Chunks.TryGetChunk(chunkPoint, out Chunk chunk) : Chunks.TryGetOrCreateChunk(chunkPoint, out chunk!))) {
					continue;
				}

				if (!chunk.Entity.Has<ChunkDecals>()) {
					AddChunkComponent(chunk);
				}

				ref var chunkDecals = ref chunk.Entity.Get<ChunkDecals>();
				ref var styleData = ref chunkDecals.DecalStyleData[style.Id];
				uint index = styleData.NumDecalsToDraw++;

				if (index >= styleData.DecalsToDraw.Length) {
					Array.Resize(ref styleData.DecalsToDraw, (int)BitOperations.RoundUpToPowerOf2(index + 1));
				}

				styleData.DecalsToDraw[index] = decal;
			}
		}
	}

	private static void AddPendingDecals()
	{
		if (!EnableDecals)
			return;

		bool renderTargetSet = false;

		foreach (Chunk chunk in Chunks.IterateAllChunks()) {
			ref readonly var chunkInfo = ref chunk.Entity.Get<ChunkInfo>();
			ref var chunkDecals = ref chunk.Entity.Get<ChunkDecals>();

			if (chunkDecals.Texture == null || chunkDecals.DecalStyleData is not { Length: > 0 })
				return;

			var sb = Main.spriteBatch;
			var chunkPosition = chunkInfo.WorldRectangle.Position;

			for (int i = 0; i < chunkDecals.DecalStyleData.Length; i++) {
				ref var styleData = ref chunkDecals.DecalStyleData[i];

				if (styleData.NumDecalsToDraw == 0) {
					continue;
				}

				if (!renderTargetSet) {
					Main.instance.GraphicsDevice.SetRenderTarget(chunkDecals.Texture);
					renderTargetSet = true;
				}

				var style = DecalStyles[i];

				sb.Begin(SpriteSortMode.Deferred, style.BlendState, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise);

				for (int j = 0; j < styleData.NumDecalsToDraw; j++) {
					DecalInfo info = styleData.DecalsToDraw[j];
					var halfSize = (Vector2Int)(info.SrcRect?.Size() ?? info.Texture.Size()) * 0.5f;
					var halfScale = info.Scale * 0.5f;
					var origin = halfSize;
					var position = new Vector2(
						MathF.Floor((info.Position.X - chunkPosition.X) * 0.5f) + (halfSize.X % 2f != 0f ? 0.5f : 0f),
						MathF.Floor((info.Position.Y - chunkPosition.Y) * 0.5f) + (halfSize.Y % 2f != 0f ? 0.5f : 0f)
					);

					sb.Draw(info.Texture, position, info.SrcRect, info.Color, info.Rotation, origin, halfScale, 0, 0f);
				}

				sb.End();

				styleData.NumDecalsToDraw = 0;
			}
		}

		if (renderTargetSet) {
			Main.instance.GraphicsDevice.SetRenderTarget(null);
		}
	}

	private static readonly short[] QuadTriangles = { 0, 2, 3, 0, 1, 2 };

	private static void RenderDecalsInWorld()
	{
		if (!EnableDecals) return;
		if (!LightingSystem.TryGetLightingBuffer(out var lightingBuffer)) return;

		const int NumTextures = 3;
		var graphicsDevice = Main.instance.GraphicsDevice;

		var vertices = ArrayPool<VertexPositionUv3>.Shared.Rent(4);
		using var _ = new Defer(() => ArrayPool<VertexPositionUv3>.Shared.Return(vertices));

		foreach (var chunk in Chunks.IterateVisibleChunks()) {
			if (!chunk.Entity.Has<ChunkDecals>()) continue;

			ref readonly var chunkInfo = ref chunk.Entity.Get<ChunkInfo>();
			ref readonly var chunkDecals = ref chunk.Entity.Get<ChunkDecals>();

			var dstRect = chunkInfo.WorldRectangle;
			dstRect.X -= Main.screenPosition.X;
			dstRect.Y -= Main.screenPosition.Y;
			var shader = BloodShader?.Value;

			if (shader == null || chunkDecals.Texture == null || Main.instance.tileTarget == null)
				continue;

			lock (lightingBuffer) {
				shader.Parameters["texture0"].SetValue(chunkDecals.Texture);
				shader.Parameters["texture1"].SetValue(Main.instance.tileTarget);
				shader.Parameters["lightingBuffer"].SetValue(lightingBuffer);
				shader.Parameters["transformMatrix"].SetValue(Main.GameViewMatrix.NormalizedTransformationmatrix);

				graphicsDevice.BlendState = BlendState.AlphaBlend;

				foreach (var pass in shader.CurrentTechnique.Passes) {
					pass.Apply();

					var tileExtensionOffset = Main.sceneTilePos - Main.screenPosition;
					var tileTargetSize = Main.instance.tileTarget.Size();

					var pos = new Vector4(dstRect.Left, dstRect.Top, dstRect.Right, dstRect.Bottom);
					var uvDecal = new Vector4(0f, 0f, 1f, 1f);
					var uvTiles = new Vector4(
						(dstRect.Left - tileExtensionOffset.X) / tileTargetSize.X,
						(dstRect.Top - tileExtensionOffset.Y) / tileTargetSize.Y,
						(dstRect.Right - tileExtensionOffset.X) / tileTargetSize.X,
						(dstRect.Bottom - tileExtensionOffset.Y) / tileTargetSize.Y
					);
					var uvLight = new Vector4(
						dstRect.Left / (float)Main.screenWidth,
						dstRect.Top / (float)Main.screenHeight,
						dstRect.Right / (float)Main.screenWidth,
						dstRect.Bottom / (float)Main.screenHeight
					);

					vertices[0] = new(new(pos.X, pos.Y, 0f), new(uvDecal.X, uvDecal.Y), new(uvTiles.X, uvTiles.Y), new(uvLight.X, uvLight.Y));
					vertices[1] = new(new(pos.Z, pos.Y, 0f), new(uvDecal.Z, uvDecal.Y), new(uvTiles.Z, uvTiles.Y), new(uvLight.Z, uvLight.Y));
					vertices[2] = new(new(pos.Z, pos.W, 0f), new(uvDecal.Z, uvDecal.W), new(uvTiles.Z, uvTiles.W), new(uvLight.Z, uvLight.W));
					vertices[3] = new(new(pos.X, pos.W, 0f), new(uvDecal.X, uvDecal.W), new(uvTiles.X, uvTiles.W), new(uvLight.X, uvLight.W));

					graphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices, 0, vertices.Length, QuadTriangles, 0, QuadTriangles.Length / 3);
				}
			}
		}

		// It is important to unbind the textures.
		for (int i = 0; i < NumTextures; i++) {
			graphicsDevice.Textures[i] = null;
		}
	}

	private static void AddChunkComponent(Chunk chunk)
	{
		ref var chunkInfo = ref chunk.Entity.Get<ChunkInfo>();
		chunkInfo.ValuableComponentCount++;

		ref var chunkDecals = ref chunk.Entity.Add(new ChunkDecals());
		Array.Resize(ref chunkDecals.DecalStyleData, DecalStyles.Length);
		for (int i = 0; i < chunkDecals.DecalStyleData.Length; i++) {
			chunkDecals.DecalStyleData[i] = new();
		}

		ThreadUtils.RunOnMainThread(() => {
			ref readonly var chunkInfo = ref chunk.Entity.Get<ChunkInfo>();
			ref var chunkDecals = ref chunk.Entity.Get<ChunkDecals>();
			int textureWidth = chunkInfo.TileRectangle.Width * 8;
			int textureHeight = chunkInfo.TileRectangle.Height * 8;

			chunkDecals.Texture = new RenderTarget2D(Main.graphics.GraphicsDevice, textureWidth, textureHeight, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
			// Initialize with transparent data to prevent driver-specific issues.
			TextureUtils.InitializeWithColor(chunkDecals.Texture, Color.Transparent);
		});
	}
	private static void RemoveChunkComponent(Chunk chunk)
	{
		if (!chunk.Entity.Has<ChunkDecals>()) return;

		ref var chunkDecals = ref chunk.Entity.Get<ChunkDecals>();

		if (chunkDecals.Texture != null) {
			var textureHandle = chunkDecals.Texture;
			ThreadUtils.RunOnMainThread(textureHandle.Dispose);
			chunkDecals.Texture = null;
		}

		checked { chunk.Entity.Get<ChunkInfo>().ValuableComponentCount--; }
	}
}

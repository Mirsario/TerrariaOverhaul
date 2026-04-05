// Copyright (c) 2020-2026 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

// #define DEBUG_DECALS

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
using TerrariaOverhaul.Core.Lighting;
using TerrariaOverhaul.Utilities;
using TerrariaOverhaul.Utilities.Terraria;
using TerrariaOverhaul.Utilities.Xna;
using BitOperations = System.Numerics.BitOperations;

namespace TerrariaOverhaul.Common.Decals;

internal enum DecalLayer : byte
{
	Foreground,
	Background,
	Count,
}
[Flags]
internal enum DecalLayerFlags : byte
{
	None = 0,
	Foreground = 1,
	Background = 2,
	All = Foreground | Background,
}

[StructLayout(LayoutKind.Sequential)]
internal struct ChunkDecals() : IComponent
{
	private static RenderTarget2D[] masks = [];

	public DecalLayerData Foreground;
	public DecalLayerData Background;

	public static unsafe Span<DecalLayerData> Layers(ref ChunkDecals self)
		=> MemoryMarshal.CreateSpan(ref self.Foreground, (int)DecalLayer.Count);

	public static unsafe ReadOnlySpan<RenderTarget2D> LayerMasks()
	{
		Array.Resize(ref masks, (int)DecalLayer.Count);
		masks[(int)DecalLayer.Foreground] = Main.instance.tileTarget;
		masks[(int)DecalLayer.Background] = Main.instance.wallTarget;
		return masks;
	}

	public static Vector2 LayerMaskPos(DecalLayer layer)
	{
		return layer switch {
			DecalLayer.Foreground => Main.sceneTilePos,
			DecalLayer.Background => Main.sceneWallPos,
			_ => throw new NotImplementedException(),
		};
	}
}
internal struct DecalLayerData()
{
	public RenderTarget2D? Texture;
	public DecalStyleData[] Styles = [];
}
internal struct DecalStyleData()
{
	public uint NumDecalsToDraw = 0;
	public DecalInfo[] DecalsToDraw = [];
}

internal struct DecalInfo
{
	public static Texture2D DefaultTexture => TextureAssets.BlackTile.Value;

	public required DecalLayerFlags Layers;
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
internal sealed class DecalSystem : ModSystem
{
	public static readonly BlendState DefaultBlendState = BlendState.AlphaBlend;
	public static readonly ConfigEntry<bool> EnableDecals = new(ConfigSide.ClientOnly, true, "BloodAndGore");

	private static readonly List<DecalStyle> decalStyles = new();
	private static readonly Query decalChunks = Entities.Query().With<ChunkInfo>().With<ChunkDecals>();

	public static Asset<Effect>? BloodShader { get; private set; }

	public static ReadOnlySpan<DecalStyle> DecalStyles => CollectionsMarshal.AsSpan(decalStyles);

	public override void Load()
	{
		Main.OnPreDraw += OnPreDraw;
		Chunks.OnChunkDestroyed += RemoveChunkComponent;

		DecalStyle.RegisterDefaultStyles();

		ThreadUtils.RunOnMainThread(() => {
			BloodShader = Mod.Assets.Request<Effect>("Assets/Shaders/Blood");
			On_Main.DoDraw_Waterfalls += static (orig, self) => {
				orig(self);
				PostDrawWalls();
			};
		});
	}
	public override void Unload()
	{
		Main.OnPreDraw -= OnPreDraw;
	}

	private void OnPreDraw(GameTime gameTime)
	{
		DebugDecals();
		AddPendingDecals();
	}
	public override void PostDrawTiles()
	{
		RenderDecalsInWorld(DecalLayer.Foreground);
	}
	private static void PostDrawWalls()
	{
		RenderDecalsInWorld(DecalLayer.Background);
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
			Layers = DecalLayerFlags.All,
			Position = new Vector2(dst.X + dst.Width * 0.5f, dst.Y + dst.Height * 0.5f),
			Size = dst.Size(),
			Color = Color.Transparent,
			IfChunkExists = true,
		});
	}
	public static void ClearDecals(Texture2D texture, Rectangle dst, Color color)
	{
		AddDecals(DecalStyle.Subtractive, new DecalInfo {
			Layers = DecalLayerFlags.All,
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
		var rect = new Rectangle((int)aabb.X, (int)aabb.Y, (int)(aabb.Z - aabb.X) + 1, (int)(aabb.W - aabb.Y) + 1);

		//DebugSystem.DrawRectangle(rect, Color.Bisque);

		var chunkStart = new Vector2Int(
			(int)(aabb.X / WorldUtils.TileSizeInPixels / Chunks.MaxChunkSize),
			(int)(aabb.Y / WorldUtils.TileSizeInPixels / Chunks.MaxChunkSize)
		);
		var chunkEnd = new Vector2Int(
			(int)(aabb.Z / WorldUtils.TileSizeInPixels / Chunks.MaxChunkSize),
			(int)(aabb.W / WorldUtils.TileSizeInPixels / Chunks.MaxChunkSize)
		);
		var layerBitMask = new BitMask<byte>((byte)decal.Layers);

		// The provided rectangle will be rendered across all the chunks it spans.
		for (int chunkY = chunkStart.Y; chunkY <= chunkEnd.Y; chunkY++) {
			for (int chunkX = chunkStart.X; chunkX <= chunkEnd.X; chunkX++) {
				var chunkPoint = new Vector2Int(chunkX, chunkY);

				if (!(decal.IfChunkExists ? Chunks.TryGetChunk(chunkPoint, out Chunk chunk) : Chunks.TryGetOrCreateChunk(chunkPoint, out chunk!))) {
					continue;
				}

				//DebugSystem.DrawRectangle((Rectangle)chunk.Entity.Get<ChunkInfo>().WorldRectangle, Main.DiscoColor, width: 8);

				if (!chunk.Entity.Has<ChunkDecals>()) {
					AddChunkComponent(chunk);
				}

				var layers = ChunkDecals.Layers(ref chunk.Entity.Get<ChunkDecals>());
				foreach (int layerIndex in layerBitMask) {
					ref var layer = ref layers[layerIndex];
					ref var styleData = ref layer.Styles[style.Id];

					uint index = styleData.NumDecalsToDraw++;
					if (index >= styleData.DecalsToDraw.Length) {
						Array.Resize(ref styleData.DecalsToDraw, (int)BitOperations.RoundUpToPowerOf2(index + 1));
					}
					styleData.DecalsToDraw[index] = decal;
				}
			}
		}
	}

	private static void AddPendingDecals()
	{
		if (!EnableDecals)
			return;

		bool mustUnbindTarget = false;

		foreach (DataEntity chunkEntity in decalChunks) {
			ref var chunkDecals = ref chunkEntity.Get<ChunkDecals>();
			ref readonly var chunkInfo = ref chunkEntity.Get<ChunkInfo>();

			var sb = Main.spriteBatch;
			var chunkWorldPos = chunkInfo.WorldRectangle.Position;

			foreach (ref var layer in ChunkDecals.Layers(ref chunkDecals)) {
				bool chunkRenderTargetSet = false;

				for (int i = 0; i < layer.Styles.Length; i++) {
					ref var styleData = ref layer.Styles[i];

					if (styleData.NumDecalsToDraw == 0) {
						continue;
					}

					if (!chunkRenderTargetSet) {
						Main.instance.GraphicsDevice.SetRenderTarget(layer.Texture);
						mustUnbindTarget = true;
						chunkRenderTargetSet = true;
					}

					var style = DecalStyles[i];

					sb.Begin(SpriteSortMode.Deferred, style.BlendState, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise);

					for (int j = 0; j < styleData.NumDecalsToDraw; j++) {
						DecalInfo info = styleData.DecalsToDraw[j];
						var halfSize = (Vector2Int)(info.SrcRect?.Size() ?? info.Texture.Size()) * 0.5f;
						var halfScale = info.Scale * 0.5f;
						var origin = halfSize;
						var position = new Vector2(
							MathF.Floor((info.Position.X - chunkWorldPos.X) * 0.5f) + (halfSize.X % 2f != 0f ? 0.5f : 0f),
							MathF.Floor((info.Position.Y - chunkWorldPos.Y) * 0.5f) + (halfSize.Y % 2f != 0f ? 0.5f : 0f)
						);

						sb.Draw(info.Texture, position, info.SrcRect, info.Color, info.Rotation, origin, halfScale, 0, 0f);
					}

					sb.End();

					styleData.NumDecalsToDraw = 0;
				}
			}
		}

		if (mustUnbindTarget) {
			Main.instance.GraphicsDevice.SetRenderTarget(null);
		}
	}

	private static readonly short[] QuadTriangles = { 0, 2, 3, 0, 1, 2 };

	private static void RenderDecalsInWorld(DecalLayer layerIndex)
	{
		if (!EnableDecals) return;
		if (!LightingSystem.TryGetLightingBuffer(out var lightingBuffer)) return;

		const int NumTextures = 3;
		var graphicsDevice = Main.instance.GraphicsDevice;

		var maskTexture = ChunkDecals.LayerMasks()[(int)layerIndex];
		var maskPos = ChunkDecals.LayerMaskPos(layerIndex);
		var vertices = ArrayPool<VertexPositionUv3>.Shared.Rent(4);
		using var _ = new Defer(() => ArrayPool<VertexPositionUv3>.Shared.Return(vertices));

		foreach (var chunk in Chunks.IterateVisibleChunks()) {
			if (!chunk.Entity.Has<ChunkDecals>()) continue;

			ref readonly var chunkInfo = ref chunk.Entity.Get<ChunkInfo>();
			ref readonly var layer = ref ChunkDecals.Layers(ref chunk.Entity.Get<ChunkDecals>())[(int)layerIndex];

			var dstRect = chunkInfo.WorldRectangle;
			dstRect.X -= Main.screenPosition.X;
			dstRect.Y -= Main.screenPosition.Y;
			var shader = BloodShader?.Value;

			if (shader == null || maskTexture == null)
				continue;

			shader.Parameters["texture0"].SetValue(layer.Texture);
			shader.Parameters["maskTexture"].SetValue(maskTexture);
			shader.Parameters["lightingBuffer"].SetValue(lightingBuffer);
			shader.Parameters["transformMatrix"].SetValue(Main.GameViewMatrix.NormalizedTransformationmatrix);

			graphicsDevice.BlendState = BlendState.AlphaBlend;

			foreach (var pass in shader.CurrentTechnique.Passes) {
				pass.Apply();

				var maskExtensionOffset = maskPos - Main.screenPosition;
				var maskTargetSize = maskTexture.Size();

				var pos = new Vector4(dstRect.Left, dstRect.Top, dstRect.Right, dstRect.Bottom);
				var uvDecal = new Vector4(0f, 0f, 1f, 1f);
				var uvTiles = new Vector4(
					(dstRect.Left - maskExtensionOffset.X) / maskTargetSize.X,
					(dstRect.Top - maskExtensionOffset.Y) / maskTargetSize.Y,
					(dstRect.Right - maskExtensionOffset.X) / maskTargetSize.X,
					(dstRect.Bottom - maskExtensionOffset.Y) / maskTargetSize.Y
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
		foreach (ref var layer in ChunkDecals.Layers(ref chunkDecals)) {
			Array.Resize(ref layer.Styles, DecalStyles.Length);
			for (int i = 0; i < layer.Styles.Length; i++) {
				layer.Styles[i] = new();
			}
		}

		ThreadUtils.RunOnMainThread(() => {
			ref readonly var chunkInfo = ref chunk.Entity.Get<ChunkInfo>();
			ref var chunkDecals = ref chunk.Entity.Get<ChunkDecals>();
			int textureWidth = chunkInfo.TileRectangle.Width * 8;
			int textureHeight = chunkInfo.TileRectangle.Height * 8;

			foreach (ref var layer in ChunkDecals.Layers(ref chunkDecals)) {
				layer.Texture = new RenderTarget2D(Main.graphics.GraphicsDevice, textureWidth, textureHeight, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
				// Initialize with transparent data to prevent driver-specific issues.
				TextureUtils.InitializeWithColor(layer.Texture, Color.Transparent);
			}
		});
	}
	private static void RemoveChunkComponent(Chunk chunk)
	{
		if (!chunk.Entity.Has<ChunkDecals>()) return;

		ref var chunkDecals = ref chunk.Entity.Get<ChunkDecals>();

		foreach (ref var layer in ChunkDecals.Layers(ref chunkDecals)) {
			if (layer.Texture != null) {
				var handle = layer.Texture;
				ThreadUtils.RunOnMainThread(handle.Dispose);
				layer.Texture = null;
			}
		}

		checked { chunk.Entity.Get<ChunkInfo>().ValuableComponentCount--; }
	}

	private static void DebugDecals()
	{
#if DEBUG_DECALS // Decal debugging hotkey.
		if (!Main.dedServ && Core.Input.InputSystem.GetKey(Microsoft.Xna.Framework.Input.Keys.K)) {
			AddDecals(DecalStyle.Default, new DecalInfo {
				Layers = DecalLayerFlags.All,
				Position = Main.MouseWorld,
				Texture = ModContent.Request<Texture2D>($"{nameof(TerrariaOverhaul)}/Content/Menus/Logo", AssetRequestMode.ImmediateLoad).Value,
				Scale = new Vector2(1f, 1f),
			});
		}
#endif
	}
}

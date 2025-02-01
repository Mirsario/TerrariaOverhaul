// Copyright (c) 2020-2025 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using TerrariaOverhaul.Core.Chunks;
using TerrariaOverhaul.Core.DataStructures;
using TerrariaOverhaul.Utilities;
using BitOperations = System.Numerics.BitOperations;

namespace TerrariaOverhaul.Common.Decals;

[Autoload(Side = ModSide.Client)]
public sealed class ChunkDecals : ChunkComponent
{
	private struct DecalStyleData
	{
		public uint NumDecalsToDraw = 0;
		public DecalInfo[] DecalsToDraw = new DecalInfo[MinDecalBufferSize];

		public DecalStyleData() { }
	}

	private const int MinDecalBufferSize = 64;

	private static readonly short[] QuadTriangles = { 0, 2, 3, 0, 1, 2 };

	private RenderTarget2D? texture;
	private DecalStyleData[] decalStyleData = Array.Empty<DecalStyleData>();

	public override void OnInit(Chunk chunk)
	{
		int textureWidth = chunk.TileRectangle.Width * 8;
		int textureHeight = chunk.TileRectangle.Height * 8;

		Array.Resize(ref decalStyleData, DecalSystem.DecalStyles.Length);

		for (int i = 0; i < decalStyleData.Length; i++) {
			decalStyleData[i] = new();
		}

		Main.QueueMainThreadAction(() => {
			texture = new RenderTarget2D(Main.graphics.GraphicsDevice, textureWidth, textureHeight, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
			
			texture.InitializeWithColor(Color.Transparent); // Initialize with transparent data to prevent driver-specific issues.
		});
	}

	public override void OnDispose(Chunk chunk)
	{
		if (texture != null) {
			var textureHandle = texture;

			Main.QueueMainThreadAction(() => {
				textureHandle.Dispose();
			});

			texture = null;
		}
	}

	public override void PreGameDraw(Chunk chunk)
	{
		// Add pending decals
		
		if (!DecalSystem.EnableDecals || decalStyleData == null || texture == null) {
			return;
		}

		bool renderTargetSet = false;
		var sb = Main.spriteBatch;
		var chunkPosition = chunk.WorldRectangle.Position;

		for (int i = 0; i < decalStyleData.Length; i++) {
			ref var styleData = ref decalStyleData[i];

			if (styleData.NumDecalsToDraw == 0) {
				continue;
			}

			if (!renderTargetSet) {
				Main.instance.GraphicsDevice.SetRenderTarget(texture);

				renderTargetSet = true;
			}

			var style = DecalSystem.DecalStyles[i];

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

		if (renderTargetSet) {
			Main.instance.GraphicsDevice.SetRenderTarget(null);
		}
	}

	public override void PostDrawTiles(Chunk chunk, SpriteBatch sb)
	{
		// Render the RT in the world

		if (!DecalSystem.EnableDecals || !chunk.Components.TryGet(out ChunkLighting? lighting)) {
			return;
		}

		var destination = chunk.WorldRectangle;

		destination.X -= Main.screenPosition.X;
		destination.Y -= Main.screenPosition.Y;

		var shader = DecalSystem.BloodShader?.Value;
		var lightingBuffer = lighting.Texture;

		if (shader == null || texture == null || lightingBuffer == null || Main.instance.tileTarget == null) {
			return;
		}

		var graphicsDevice = Main.instance.GraphicsDevice;

		lock (lightingBuffer) {
			const int NumTextures = 3;


			shader.Parameters["texture0"].SetValue(texture);
			shader.Parameters["texture1"].SetValue(Main.instance.tileTarget);
			shader.Parameters["lightingBuffer"].SetValue(lightingBuffer);
			shader.Parameters["transformMatrix"].SetValue(Main.GameViewMatrix.NormalizedTransformationmatrix);

			graphicsDevice.BlendState = BlendState.AlphaBlend;

			foreach (var pass in shader.CurrentTechnique.Passes) {
				pass.Apply();

				//TODO: Comment the following.
				var tOffset = Main.sceneTilePos - Main.screenPosition;
				var vec = new Vector2(
					chunk.WorldRectangle.Width / Main.instance.tileTarget.Width / chunk.WorldRectangle.Width,
					chunk.WorldRectangle.Height / Main.instance.tileTarget.Height / chunk.WorldRectangle.Height
				);
				var vertices = new[] {
					new VertexPositionUv2(new Vector3(destination.Left, destination.Top, 0f), new Vector2(0f, 0f), (destination.TopLeft - tOffset) * vec),
					new VertexPositionUv2(new Vector3(destination.Right, destination.Top, 0f), new Vector2(1f, 0f), (destination.TopRight - tOffset) * vec),
					new VertexPositionUv2(new Vector3(destination.Right, destination.Bottom, 0f), new Vector2(1f, 1f), (destination.BottomRight - tOffset) * vec),
					new VertexPositionUv2(new Vector3(destination.Left, destination.Bottom, 0f), new Vector2(0f, 1f), (destination.BottomLeft - tOffset) * vec)
				};

				graphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices, 0, vertices.Length, QuadTriangles, 0, QuadTriangles.Length / 3);
			}

			// Very important to unbind the textures.
			for (int i = 0; i < NumTextures; i++) {
				graphicsDevice.Textures[i] = null;
			}
		}
	}

	public void AddDecals(DecalStyle decalStyle, in DecalInfo decalInfo)
	{
		ref var styleData = ref decalStyleData[decalStyle.Id];
		uint index = styleData.NumDecalsToDraw++;

		if (index >= styleData.DecalsToDraw.Length) {
			Array.Resize(ref styleData.DecalsToDraw, (int)BitOperations.RoundUpToPowerOf2(index + 1));
		}

		styleData.DecalsToDraw[index] = decalInfo;
	}
}

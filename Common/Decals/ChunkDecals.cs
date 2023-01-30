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
	private struct DecalInfo
	{
		public Texture2D texture;
		public Rectangle destRect;
		public Rectangle? srcRect;
		public Color color;

		public DecalInfo(Texture2D texture, Rectangle destRect, Rectangle? srcRect, Color color)
		{
			this.texture = texture;
			this.destRect = destRect;
			this.srcRect = srcRect;
			this.color = color;
		}
	}

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
		
		if (decalStyleData == null || texture == null) {
			return;
		}

		bool renderTargetSet = false;
		var sb = Main.spriteBatch;

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

				sb.Draw(info.texture, info.destRect, info.srcRect, info.color);
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

		if (!chunk.Components.TryGet(out ChunkLighting? lighting)) {
			return;
		}

		var destination = chunk.WorldRectangle;

		destination.x -= Main.screenPosition.X;
		destination.y -= Main.screenPosition.Y;

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
			//shader.Parameters["transformMatrix"].SetValue(Main.GameViewMatrix.TransformationMatrix);
			shader.Parameters["transformMatrix"].SetValue(GetDefaultMatrix() * Matrix.CreateScale(Main.ForcedMinimumZoom));

			graphicsDevice.BlendState = BlendState.AlphaBlend;

			foreach (var pass in shader.CurrentTechnique.Passes) {
				pass.Apply();

				//TODO: Comment the following.
				var tOffset = Main.sceneTilePos - Main.screenPosition;
				var vec = new Vector2(
					chunk.WorldRectangle.width / Main.instance.tileTarget.Width / chunk.WorldRectangle.width,
					chunk.WorldRectangle.height / Main.instance.tileTarget.Height / chunk.WorldRectangle.height
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

	public void AddDecals(DecalStyle decalStyle, Texture2D texture, Rectangle localDestRect, Rectangle? srcRect, Color color)
	{
		ref var styleData = ref decalStyleData[decalStyle.Id];
		uint index = styleData.NumDecalsToDraw++;

		if (index >= styleData.DecalsToDraw.Length) {
			Array.Resize(ref styleData.DecalsToDraw, (int)BitOperations.RoundUpToPowerOf2(index + 1));
		}

		styleData.DecalsToDraw[index] = new DecalInfo(texture, localDestRect, srcRect, color);
	}

	private static Matrix GetDefaultMatrix()
	{
		float num = Main.screenWidth > 0 ? 1f / Main.screenWidth : 0f;
		float num2 = Main.screenHeight > 0 ? -1f / Main.screenHeight : 0f;

		var matrix = default(Matrix);

		matrix.M11 = num * 2f;
		matrix.M22 = num2 * 2f;
		matrix.M33 = 1f;
		matrix.M44 = 1f;
		matrix.M41 = -1f;
		matrix.M42 = 1f;
		matrix.M41 -= num;
		matrix.M42 -= num2;

		matrix *= Matrix.CreateScale(Main.GameZoomTarget);

		return matrix;
	}
}

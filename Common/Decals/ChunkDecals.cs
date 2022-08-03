using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using TerrariaOverhaul.Core.Chunks;
using TerrariaOverhaul.Core.DataStructures;
using TerrariaOverhaul.Utilities;

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

	private static readonly short[] QuadTriangles = { 0, 2, 3, 0, 1, 2 };

	private RenderTarget2D? texture;
	private Dictionary<BlendState, List<DecalInfo>>? decalsToAdd;

	public override void OnInit(Chunk chunk)
	{
		int textureWidth = chunk.TileRectangle.Width * 8;
		int textureHeight = chunk.TileRectangle.Height * 8;

		decalsToAdd = new Dictionary<BlendState, List<DecalInfo>>();

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
		
		if (decalsToAdd == null || decalsToAdd.Count == 0 || texture == null) {
			return;
		}

		Main.instance.GraphicsDevice.SetRenderTarget(texture);

		var sb = Main.spriteBatch;

		foreach (var pair in decalsToAdd) {
			var blendState = pair.Key;
			var drawList = pair.Value;

			sb.Begin(SpriteSortMode.Deferred, blendState, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise);

			foreach (var info in drawList) {
				sb.Draw(info.texture, info.destRect, info.srcRect, info.color);
			}

			sb.End();
		}

		Main.instance.GraphicsDevice.SetRenderTarget(null);

		decalsToAdd.Clear();
	}

	public override void PostDrawTiles(Chunk chunk, SpriteBatch sb)
	{
		// Render the RT in the world

		var destination = chunk.WorldRectangle;

		destination.x -= Main.screenPosition.X;
		destination.y -= Main.screenPosition.Y;

		var shader = DecalSystem.BloodShader?.Value;
		var lightingBuffer = chunk.Components.Get<ChunkLighting>().Texture;

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

	public void AddDecals(Texture2D texture, Rectangle localDestRect, Rectangle? srcRect, Color color, BlendState blendState)
	{
		if (decalsToAdd == null) {
			return;
		}

		if (!decalsToAdd.TryGetValue(blendState, out var list)) {
			decalsToAdd[blendState] = list = new List<DecalInfo>();
		}

		list.Add(new DecalInfo(texture, localDestRect, srcRect, color));
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

using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using TerrariaOverhaul.Core.DataStructures;
using TerrariaOverhaul.Core.Systems.Chunks;

namespace TerrariaOverhaul.Common.Systems.Decals
{
	[Autoload(Side = ModSide.Client)]
	public sealed class ChunkDecals : ChunkComponent
	{
		private static readonly short[] QuadTriangles = { 0, 2, 3, 0, 1, 2 };

		private RenderTarget2D texture;
		private Dictionary<Rectangle, Color> decalsToAdd;

		public override void OnInit()
		{
			int textureWidth = Chunk.TileRectangle.Width * 8;
			int textureHeight = Chunk.TileRectangle.Height * 8;

			texture = new RenderTarget2D(Main.graphics.GraphicsDevice, textureWidth, textureHeight, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
			decalsToAdd = new Dictionary<Rectangle, Color>();
		}
		public override void OnDispose()
		{
			if(texture != null) {
				texture.Dispose();

				texture = null;
			}
		}
		public override void PreGameDraw()
		{
			//Add pending decals

			if(decalsToAdd.Count == 0) {
				return;
			}

			Main.instance.GraphicsDevice.SetRenderTarget(texture);

			var sb = Main.spriteBatch;

			sb.Begin(SpriteSortMode.Deferred, BlendState.Opaque, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise);

			foreach(var pair in decalsToAdd) {
				var color = pair.Value;

				color.A = 255;

				sb.Draw(TextureAssets.BlackTile.Value, pair.Key, color);
			}

			sb.End();

			Main.instance.GraphicsDevice.SetRenderTarget(null);

			decalsToAdd.Clear();
		}
		public override void PostDrawTiles(SpriteBatch sb)
		{
			//Render the RT in the world

			var destination = Chunk.WorldRectangle;

			destination.X -= (int)Main.screenPosition.X;
			destination.Y -= (int)Main.screenPosition.Y;

			//sb.Draw(texture, destination, Color.White);

			var shader = DecalSystem.BloodShader.Value;

			shader.Parameters["texture0"].SetValue(texture);
			shader.Parameters["texture1"].SetValue(Main.instance.tileTarget);
			shader.Parameters["lightingBuffer"].SetValue(TextureAssets.MagicPixel.Value); //(lightingBuffer);
			//shader.Parameters["transformMatrix"].SetValue(Main.GameViewMatrix.TransformationMatrix);
			shader.Parameters["transformMatrix"].SetValue(GetDefaultMatrix() * Matrix.CreateScale(Main.ForcedMinimumZoom));

			Main.instance.GraphicsDevice.BlendState = BlendState.AlphaBlend;

			foreach(var pass in shader.CurrentTechnique.Passes) {
				pass.Apply();

				//TODO: Comment the following.
				var tOffset = Main.sceneTilePos - Main.screenPosition;
				var vec = new Vector2(
					Chunk.WorldRectangle.Width / (float)Main.instance.tileTarget.Width / Chunk.WorldRectangle.Width,
					Chunk.WorldRectangle.Height / (float)Main.instance.tileTarget.Height / Chunk.WorldRectangle.Height
				);
				var vertices = new[] {
					new VertexPositionUv2(new Vector3(destination.Left, destination.Top, 0f), new Vector2(0f, 0f), (destination.TopLeft() - tOffset) * vec),
					new VertexPositionUv2(new Vector3(destination.Right, destination.Top, 0f), new Vector2(1f, 0f), (destination.TopRight() - tOffset) * vec),
					new VertexPositionUv2(new Vector3(destination.Right, destination.Bottom, 0f), new Vector2(1f, 1f), (destination.BottomRight() - tOffset) * vec),
					new VertexPositionUv2(new Vector3(destination.Left, destination.Bottom, 0f), new Vector2(0f, 1f), (destination.BottomLeft() - tOffset) * vec)
				};

				Main.instance.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices, 0, vertices.Length, QuadTriangles, 0, QuadTriangles.Length / 3);
			}
		}

		public void AddDecals(Rectangle localRect, Color color) => decalsToAdd[localRect] = color;

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
}

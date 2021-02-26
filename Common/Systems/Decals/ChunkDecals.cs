using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.Systems.Lighting;
using TerrariaOverhaul.Core.DataStructures;
using TerrariaOverhaul.Core.Systems.Chunks;
using TerrariaOverhaul.Core.Systems.Debugging;

namespace TerrariaOverhaul.Common.Systems.Decals
{
	[Autoload(Side = ModSide.Client)]
	public sealed class ChunkDecals : ChunkComponent
	{
		private static readonly short[] QuadTriangles = { 0, 2, 3, 0, 1, 2 };

		private RenderTarget2D texture;
		private List<(Texture2D texture, Rectangle destRect, Rectangle? srcRect, Color color)> decalsToAdd;

		public override void OnInit()
		{
			int textureWidth = Chunk.TileRectangle.Width * 8;
			int textureHeight = Chunk.TileRectangle.Height * 8;

			texture = new RenderTarget2D(Main.graphics.GraphicsDevice, textureWidth, textureHeight, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
			decalsToAdd = new List<(Texture2D, Rectangle, Rectangle?, Color)>();
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

			sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise);

			foreach(var tuple in decalsToAdd) {
				sb.Draw(tuple.texture, tuple.destRect, tuple.srcRect, tuple.color);
			}

			sb.End();

			Main.instance.GraphicsDevice.SetRenderTarget(null);

			decalsToAdd.Clear();
		}
		public override void PostDrawTiles(SpriteBatch sb)
		{
			//Render the RT in the world

			var destination = Chunk.WorldRectangle;

			destination.x -= Main.screenPosition.X;
			destination.y -= Main.screenPosition.Y;

			//sb.Draw(texture, destination, Color.White);

			bool CheckResources(params (GraphicsResource, string)[] resources)
			{
				foreach(var (resource, name) in resources) {
					if(resource.IsDisposed != false) {
						DebugSystem.Log($"{nameof(ChunkDecals)}.{nameof(PostDrawTiles)}: {name} is null or disposed.");

						return false;
					}
				}

				return true;
			}

			var shader = DecalSystem.BloodShader.Value;
			var lightingBuffer = Chunk.GetComponent<ChunkLighting>().Texture;

			if(!CheckResources(
				(texture, nameof(texture)),
				(shader, nameof(shader)),
				(lightingBuffer, nameof(lightingBuffer)),
				(Main.instance.tileTarget, nameof(Main.instance.tileTarget)),
				(TextureAssets.MagicPixel.Value, nameof(TextureAssets.MagicPixel))
			)) {
				return;
			}

			shader.Parameters["texture0"].SetValue(texture);
			shader.Parameters["texture1"].SetValue(Main.instance.tileTarget);
			shader.Parameters["lightingBuffer"].SetValue(lightingBuffer);
			//shader.Parameters["transformMatrix"].SetValue(Main.GameViewMatrix.TransformationMatrix);
			shader.Parameters["transformMatrix"].SetValue(GetDefaultMatrix() * Matrix.CreateScale(Main.ForcedMinimumZoom));

			Main.instance.GraphicsDevice.BlendState = BlendState.AlphaBlend;

			foreach(var pass in shader.CurrentTechnique.Passes) {
				pass.Apply();

				//TODO: Comment the following.
				var tOffset = Main.sceneTilePos - Main.screenPosition;
				var vec = new Vector2(
					Chunk.WorldRectangle.width / Main.instance.tileTarget.Width / Chunk.WorldRectangle.width,
					Chunk.WorldRectangle.height / Main.instance.tileTarget.Height / Chunk.WorldRectangle.height
				);
				var vertices = new[] {
					new VertexPositionUv2(new Vector3(destination.Left, destination.Top, 0f), new Vector2(0f, 0f), (destination.TopLeft - tOffset) * vec),
					new VertexPositionUv2(new Vector3(destination.Right, destination.Top, 0f), new Vector2(1f, 0f), (destination.TopRight - tOffset) * vec),
					new VertexPositionUv2(new Vector3(destination.Right, destination.Bottom, 0f), new Vector2(1f, 1f), (destination.BottomRight - tOffset) * vec),
					new VertexPositionUv2(new Vector3(destination.Left, destination.Bottom, 0f), new Vector2(0f, 1f), (destination.BottomLeft - tOffset) * vec)
				};

				Main.instance.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices, 0, vertices.Length, QuadTriangles, 0, QuadTriangles.Length / 3);
			}
		}

		public void AddDecals(Texture2D texture, Rectangle localDestRect, Rectangle? srcRect, Color color) => decalsToAdd.Add((texture, localDestRect, srcRect, color));

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

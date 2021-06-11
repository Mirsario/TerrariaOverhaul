using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria.UI;

namespace TerrariaOverhaul.Core.Systems.Debugging
{
	partial class DebugSystem
	{
		private struct Line
		{
			public Vector2 start;
			public Vector2 end;
			public Color color;
			public int width;

			public Line(Vector2 start, Vector2 end, Color color, int width)
			{
				this.start = start;
				this.end = end;
				this.color = color;
				this.width = width;
			}
		}

		public static bool EnableDebugRendering { get; set; }
		
		private readonly List<Line> LinesToDraw = new();

		public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
		{
			layers.Add(new LegacyGameInterfaceLayer($"{nameof(TerrariaOverhaul)}/Debug", () => {
				if(EnableDebugRendering) {
					for(int i = 0; i < Main.maxPlayers; i++) {
						var player = Main.player[i];

						if(player.active) {
							DrawRectangle(player.getRect(), Color.SpringGreen, 1);
						}
					}

					for(int i = 0; i < Main.maxNPCs; i++) {
						var npc = Main.npc[i];

						if(npc.active) {
							DrawRectangle(npc.getRect(), Color.IndianRed, 1);
						}
					}

					for(int i = 0; i < Main.maxProjectiles; i++) {
						var projectile = Main.projectile[i];

						if(projectile.active) {
							DrawRectangle(projectile.getRect(), Color.Orange, 1);
						}
					}

					for(int i = 0; i < Main.maxItems; i++) {
						var item = Main.item[i];

						if(item.active) {
							DrawRectangle(item.getRect(), Color.DodgerBlue, 1);
						}
					}

					for(int i = 0; i < Main.maxGore; i++) {
						var gore = Main.gore[i];

						if(gore.active) {
							DrawRectangle(gore.AABBRectangle, Color.Purple, 1);
						}
					}

					foreach(var line in LinesToDraw) {
						Vector2 edge = line.end - line.start;
						Rectangle rect = new Rectangle(
							(int)Math.Round(line.start.X - Main.screenPosition.X),
							(int)Math.Round(line.start.Y - Main.screenPosition.Y),
							(int)edge.Length(),
							line.width
						);

						Main.spriteBatch.Draw(TextureAssets.BlackTile.Value, rect, null, line.color, (float)Math.Atan2(edge.Y, edge.X), Vector2.Zero, SpriteEffects.None, 0f);
					}
				}

				LinesToDraw.Clear();

				return true;
			}, InterfaceScaleType.Game));
		}
		
		public override void PostDrawInterface(SpriteBatch sb)
		{
			
		}

		public static void DrawLine(Vector2 start, Vector2 end, Color color, int width = 2)
		{
			ModContent.GetInstance<DebugSystem>().LinesToDraw.Add(new Line(start, end, color, width));
		}
		
		public static void DrawRectangle(Rectangle rectangle, Color color, int width = 2)
		{
			var lines = ModContent.GetInstance<DebugSystem>().LinesToDraw;

			lines.Add(new Line(rectangle.TopLeft(), rectangle.TopRight(), color, width));
			lines.Add(new Line(rectangle.TopRight(), rectangle.BottomRight(), color, width));
			lines.Add(new Line(rectangle.BottomRight(), rectangle.BottomLeft(), color, width));
			lines.Add(new Line(rectangle.BottomLeft(), rectangle.TopLeft(), color, width));
		}
		
		public static void DrawCircle(Vector2 center, float radius, Color color, int resolution = 16, int width = 2)
		{
			var lines = ModContent.GetInstance<DebugSystem>().LinesToDraw;

			float step = MathHelper.TwoPi / resolution;
			Vector2 offset = new Vector2(radius, 0f);

			for(int i = 0; i <= resolution; i++) {
				Line line;

				line.start = center + offset;
				line.end = center + (offset = offset.RotatedBy(step));
				line.color = color;
				line.width = width;

				lines.Add(line);
			}
		}
	}
}

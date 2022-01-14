using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using Terraria.GameContent;

namespace TerrariaOverhaul.Utilities.Extensions
{
	public static class SpriteBatchExtensions
	{
		public static void DrawLine(this SpriteBatch sb, Vector2 start, Vector2 end, Color? color = null, int width = 1, Texture2D texture = null)
		{
			var offset = end - start;
			float angle = (float)Math.Atan2(offset.Y, offset.X);
			var rect = new Rectangle(
				(int)Math.Round(start.X),
				(int)Math.Round(start.Y),
				(int)offset.Length(),
				width
			);

			sb.Draw(texture ?? TextureAssets.BlackTile.Value, rect, null, color ?? Color.White, angle, Vector2.Zero, SpriteEffects.None, 0f);
		}

		// Text
		public static void DrawStringOutlined(this SpriteBatch sb, DynamicSpriteFont font, string text, Vector2 position, Color color, Vector2 origin = default, float scale = 1f)
		{
			for (int i = 0; i < 5; i++) {
				var newColor = Color.Black;

				if (i == 4) {
					newColor = color;
					newColor.R = (byte)((255 + newColor.R) / 2);
					newColor.G = (byte)((255 + newColor.G) / 2);
					newColor.B = (byte)((255 + newColor.B) / 2);
				}

				newColor.A = (byte)(newColor.A * 0.5f);

				Vector2 offset;

				switch (i) {
					case 0:
						offset = new Vector2(-2f, 0f);
						break;
					case 1:
						offset = new Vector2(2f, 0f);
						break;
					case 2:
						offset = new Vector2(0f, -2f);
						break;
					case 3:
						offset = new Vector2(0f, 2f);
						break;
					default:
						offset = default;
						break;
				}

				sb.DrawString(font, text, position + offset, newColor, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
			}
		}
	}
}

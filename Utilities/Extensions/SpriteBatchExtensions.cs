using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
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
	}
}

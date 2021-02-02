using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using ReLogic.Graphics;

namespace TerrariaOverhaul.Common.Systems.MainMenuOverlays
{
	public class PatreonMenuLink : MenuLink
	{
		public PatreonMenuLink(string text, string url, Asset<DynamicSpriteFont> font = null, float scale = 1, Func<bool, Color> forcedColor = null)
			: base(text, url, font, scale, forcedColor) { }

		public override void Draw(SpriteBatch sb, Vector2 position)
		{
			base.Draw(sb, position);

			sb.DrawString(Font.Value, IsHovered ? ":D" : ":)", position + new Vector2(Size.X + 15f, 6f), Color.White, MathHelper.ToRadians(60f), Vector2.Zero, 1f, SpriteEffects.None, 0f);
		}
	}
}

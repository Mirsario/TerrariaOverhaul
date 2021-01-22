using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using TerrariaOverhaul.Core.Systems.Input;
using TerrariaOverhaul.Utilities.Extensions;

namespace TerrariaOverhaul.Common.Systems.MainMenuOverlays
{
	public class MenuLine
	{
		private readonly string Text;
		private readonly float Scale;
		private readonly Func<bool, Color> ForcedColor;

		public MenuLine(string text, float scale = 1f, Func<bool, Color> forcedColor = null)
		{
			Text = text;
			Scale = scale;
			ForcedColor = forcedColor;
		}

		protected virtual void OnClicked() { }

		public virtual Vector2 Draw(SpriteBatch sb, DynamicSpriteFont font, Vector2 position)
		{
			var size = font.MeasureString(Text) * Scale;
			var rect = new Rectangle((int)position.X, (int)position.Y, (int)size.X, (int)size.Y);
			bool isHovering = false;

			if(rect.Contains(new Point(Main.mouseX, Main.mouseY))) {
				isHovering = true;

				if(InputSystem.GetMouseButtonDown(0)) {
					SoundEngine.PlaySound(SoundID.MenuOpen);
					OnClicked();
				}
			}

			var color = ForcedColor?.Invoke(isHovering) ?? Color.White;

			sb.DrawStringOutlined(font, Text, position, color);

			return size;
		}
	}
}

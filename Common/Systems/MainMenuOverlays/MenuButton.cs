using System;
using Microsoft.Xna.Framework;
using ReLogic.Content;
using ReLogic.Graphics;

namespace TerrariaOverhaul.Common.Systems.MainMenuOverlays
{
	public abstract class MenuButton : MenuLine
	{
		public MenuButton(string text, Asset<DynamicSpriteFont> font = null, float scale = 1f, Func<bool, Color> forcedColor = null)
			: base(text, font, scale, forcedColor ?? GetColor) { }

		protected abstract override void OnClicked();

		private static Color GetColor(bool isHovering) => isHovering ? Color.White : Color.SlateGray;
	}
}

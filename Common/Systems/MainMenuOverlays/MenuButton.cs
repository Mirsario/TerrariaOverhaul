using System;
using Microsoft.Xna.Framework;

namespace TerrariaOverhaul.Common.Systems.MainMenuOverlays
{
	public abstract class MenuButton : MenuLine
	{
		public MenuButton(string text, float scale = 1f, Func<bool, Color> forcedColor = null) : base(text, scale, forcedColor ?? GetColor) { }

		protected abstract override void OnClicked();

		private static Color GetColor(bool isHovering) => isHovering ? Color.White : Color.SlateGray;
	}
}

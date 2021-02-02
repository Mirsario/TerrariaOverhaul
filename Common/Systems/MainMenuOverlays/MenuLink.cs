using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using ReLogic.Content;
using ReLogic.Graphics;

namespace TerrariaOverhaul.Common.Systems.MainMenuOverlays
{
	public class MenuLink : MenuButton
	{
		private readonly string Url;

		public MenuLink(string text, string url, Asset<DynamicSpriteFont> font = null, float scale = 1f, Func<bool, Color> forcedColor = null)
			: base(text, font, scale, forcedColor)
		{
			Url = url;
		}

		protected override void OnClicked() => Process.Start(Url);
	}
}

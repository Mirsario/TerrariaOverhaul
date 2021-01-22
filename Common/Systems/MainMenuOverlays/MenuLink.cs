using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;

namespace TerrariaOverhaul.Common.Systems.MainMenuOverlays
{
	public class MenuLink : MenuButton
	{
		private readonly string Url;

		public MenuLink(string text, string url, float scale = 1f, Func<bool, Color> forcedColor = null) : base(text, scale, forcedColor)
		{
			Url = url;
		}

		protected override void OnClicked() => Process.Start(Url);
	}
}

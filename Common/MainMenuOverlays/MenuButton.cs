using Microsoft.Xna.Framework;

namespace TerrariaOverhaul.Common.MainMenuOverlays
{
	public abstract class MenuButton : MenuLine
	{
		public MenuButton(string text) : base(text)
		{
			ForcedColor ??= GetColor;
		}

		protected abstract override void OnClicked();

		private static Color GetColor(bool isHovering) => isHovering ? Color.White : Color.LightSlateGray;
	}
}

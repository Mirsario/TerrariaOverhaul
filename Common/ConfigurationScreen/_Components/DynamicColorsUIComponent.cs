using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;
using TerrariaOverhaul.Core.Interface;

namespace TerrariaOverhaul.Common.ConfigurationScreen;

public sealed class DynamicColorsUIComponent : UIComponent
{
	public struct Colors
	{
		public Color Normal { get; set; }
		public Color? Active { get; set; }
		public Color? Hover { get; set; }
	}

	public Colors Border;
	public Colors Background;

	private ref Color CurrentBorderColor => ref ((UIPanel)Element).BorderColor;
	private ref Color CurrentBackgroundColor => ref ((UIPanel)Element).BackgroundColor;

	protected override void OnAttach()
	{
		if (Element is not UIPanel) {
			throw new InvalidOperationException($"{GetType().Name} only supports attachment to {nameof(UIPanel)}-deriving elements.");
		}

		if (Border.Normal == default) {
			Border.Normal = CurrentBorderColor;
		}

		if (Background.Normal == default) {
			Background.Normal = CurrentBackgroundColor;
		}

		Element.OnUpdate += OnUpdate;
	}

	protected override void OnDetach()
		=> Element.OnUpdate -= OnUpdate;

	private void OnUpdate(UIElement element)
		=> UpdateBorderColor();

	private void UpdateBorderColor()
	{
		bool isHovered = Element.ContainsPoint(Main.MenuUI.MousePosition);
		bool isPressed = isHovered && Main.mouseLeft;

		Color GetColor(Colors colors)
			=> (isPressed ? colors.Active : null) ?? (isHovered ? colors.Hover : null) ?? colors.Normal;

		CurrentBorderColor = GetColor(Border);
		CurrentBackgroundColor = GetColor(Background);
	}
}

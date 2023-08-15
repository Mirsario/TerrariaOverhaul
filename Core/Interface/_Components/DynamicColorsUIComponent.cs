using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace TerrariaOverhaul.Core.Interface;

public sealed class DynamicColorsUIComponent : UIComponent
{
	public UIColors Border;
	public UIColors Background;

	private ref Color CurrentBorderColor => ref ((UIPanel)Element).BorderColor;
	private ref Color CurrentBackgroundColor => ref ((UIPanel)Element).BackgroundColor;

	public Color? OverrideBorderColor = null;
	public Color? OverrideBackgroundColor = null;

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

		Color GetColor(UIColors colors)
			=> (isPressed ? colors.Active : null) ?? (isHovered ? colors.Hover : null) ?? colors.Normal;

		CurrentBorderColor = OverrideBorderColor ?? GetColor(Border);
		CurrentBackgroundColor = OverrideBackgroundColor ?? GetColor(Background);
	}
}

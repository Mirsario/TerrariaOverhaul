using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;
using TerrariaOverhaul.Core.Input;
using TerrariaOverhaul.Core.Interface;

namespace TerrariaOverhaul.Common.ConfigurationScreen;

public sealed class BorderColorsUIComponent : UIComponent
{
	private Color normal;
	private Color? hover;

	public Color? Active { get; set; }

	public Color Normal {
		get => normal;
		set {
			normal = value;

			UpdateBorderColor();
		}
	}
	public Color? Hover {
		get => hover;
		set {
			hover = value;

			UpdateBorderColor();
		}
	}

	protected override void OnAttach()
		=> Element.OnUpdate += OnUpdate;

	protected override void OnDetach()
		=> Element.OnUpdate -= OnUpdate;

	private void OnUpdate(UIElement element)
		=> UpdateBorderColor();

	private void UpdateBorderColor()
	{
		bool isHovered = Element.ContainsPoint(Main.MenuUI.MousePosition);
		bool isPressed = isHovered && Main.mouseLeft;
		var panel = (UIPanel)Element;

		if (isPressed && Active.HasValue) {
			panel.BorderColor = Active.Value;
		} else if (isHovered && Hover.HasValue) {
			panel.BorderColor = Hover.Value;
		} else {
			panel.BorderColor = Normal;
		}
	}
}

using System;
using Microsoft.Xna.Framework;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.UI;
using TerrariaOverhaul.Core.Interface;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.ConfigurationScreen;

public class ToggleElement : UIElement, IConfigEntryController
{
	private static UIPanelColors colorOn = new() {
		Background = new(ColorUtils.FromHexRgb(0x6cb622)),
		Border = new(ColorUtils.FromHexRgb(0x0b450b), Hover: Color.White),
	};
	private static UIPanelColors colorOff = new() {
		Background = new(ColorUtils.FromHexRgb(0xbc2323)),
		Border = new(ColorUtils.FromHexRgb(0x450c29), Hover: Color.White),
	};

	private readonly FancyUIPanel statePanel;
	private readonly UIText textOn;
	private readonly UIText textOff;

	public bool Value { get; set; }

	object? IConfigEntryController.Value {
		get => Value;
		set {
			Value = (bool)value!;
			UpdateState();
		}
	}

	public event Action? OnModified;

	public ToggleElement()
	{
		MaxWidth = Width = StyleDimension.FromPixels(136f);
		MaxHeight = Height = StyleDimension.FromPercent(1.0f);

		var container = this.AddElement(new UIElement().With(e => {
			e.MaxWidth = e.Width = StyleDimension.Fill;
			e.MaxHeight = e.Height = StyleDimension.Fill;

			e.SetPadding(4f);
		}));

		var background = container.AddElement(new UIPanel().With(e => {
			e.BorderColor = CommonColors.OuterPanelMedium.Border.Normal;
			e.BackgroundColor = CommonColors.OuterPanelMedium.Border.Normal;

			e.HAlign = 0.5f;
			e.VAlign = 0.5f;
			e.MaxWidth = e.Width = StyleDimension.FromPixelsAndPercent(-8f, 1.0f);
			e.MaxHeight = e.Height = StyleDimension.FromPixelsAndPercent(-10f, 1.0f);
		}));

		statePanel = container.AddElement(new FancyUIPanel().With(e => {
			e.MaxWidth = e.Width = StyleDimension.FromPercent(0.5f);
			e.MaxHeight = e.Height = StyleDimension.FromPercent(1.0f);
		}));

		// Text.
		// NOTE: TextOrigin fields are useless.

		textOff = container.AddElement(new UIText("Off").With(e => {
			e.Recalculate();

			var dimensions = e.GetDimensions();

			e.Top = StyleDimension.FromPixelsAndPercent(-dimensions.Height * 0.5f, 0.5f);
			e.Left = StyleDimension.FromPixelsAndPercent(-dimensions.Width * 0.5f, 0.25f);
		}));

		textOn = container.AddElement(new UIText("On").With(e => {
			e.Recalculate();

			var dimensions = e.GetDimensions();

			e.Top = StyleDimension.FromPixelsAndPercent(-dimensions.Height * 0.5f, 0.5f);
			e.Left = StyleDimension.FromPixelsAndPercent(-dimensions.Width * 0.5f, 0.75f);
		}));

		UpdateState();
	}

	public override void LeftMouseDown(UIMouseEvent evt)
	{
		base.LeftMouseDown(evt);

		SoundEngine.PlaySound(SoundID.MenuTick);

		Value = !Value;

		OnModified?.Invoke();
		UpdateState();
	}

	private void UpdateState()
	{
		bool value = Value;
		var currentColors = value ? colorOn : colorOff;

		statePanel.Colors.Border = currentColors.Border;
		statePanel.Colors.Background = currentColors.Background;
		statePanel.Left = StyleDimension.FromPercent(value ? 0.5f : 0.0f);

		var textColorActive = Color.White;
		var textColorInactive = ColorUtils.FromHexRgb(0x5c5c5c);

		textOn.TextColor = value ? textColorActive : textColorInactive;
		textOff.TextColor = value ? textColorInactive : textColorActive;

		RecalculateChildren();
	}
}

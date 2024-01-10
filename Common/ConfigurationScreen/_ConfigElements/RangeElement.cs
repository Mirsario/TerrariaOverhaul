using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.UI;
using TerrariaOverhaul.Core.Interface;
using TerrariaOverhaul.Core.Time;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.ConfigurationScreen;

public class RangeElement : UIElement, IConfigEntryController
{
	private readonly UIElement container;
	private readonly UIPanel background;
	private readonly FancyUIPanel statePanel;
	private readonly UIText text;
	private bool dragging;
	private float soundCooldownEndTime;

	public double Value { get; set; }

	object? IConfigEntryController.Value {
		get => Value;
		set {
			Value = Convert.ToDouble(value!);
			UpdateState();
		}
	}

	public event Action? OnModified;

	public RangeElement()
	{
		MaxWidth = Width = StyleDimension.FromPixels(136f);
		MaxHeight = Height = StyleDimension.FromPercent(1.0f);

		container = this.AddElement(new UIElement().With(e => {
			e.MaxWidth = e.Width = StyleDimension.Fill;
			e.MaxHeight = e.Height = StyleDimension.Fill;

			e.SetPadding(4f);
		}));

		background = container.AddElement(new UIPanel().With(e => {
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

		text = container.AddElement(new UIText("0.00").With(t => {
			t.Recalculate();

			var dimensions = t.GetDimensions();

			t.Top = StyleDimension.FromPixelsAndPercent(-dimensions.Height * 0.5f, 0.5f);
			t.Left = StyleDimension.FromPixelsAndPercent(-dimensions.Width * 0.5f, 0.25f);
		}));

		UpdateState();
	}

	public override void LeftMouseDown(UIMouseEvent evt)
	{
		base.LeftMouseDown(evt);

		SoundEngine.PlaySound(SoundID.MenuTick with { Identifier = "StartStop", Pitch = 0.1f });

		dragging = true;
	}

	public override void Update(GameTime gameTime)
	{
		base.Update(gameTime);

		if (dragging) {
			var soundDrag = SoundID.MenuTick with { Identifier = "Drag", Volume = 0.6f, Pitch = 0.025f, PitchVariance = 0.1f };
			var soundStop = SoundID.MenuTick with { Identifier = "StartStop", Pitch = -0.15f, PitchVariance = 0.03f };

			if (!Main.mouseLeft) {
				dragging = false;
				OnModified?.Invoke();
				SoundEngine.PlaySound(soundStop);
			} else {
				var dimensions = GetInnerDimensions();
				var dragDimensions = dimensions with {
					X = dimensions.X + dimensions.Width * 0.25f,
					Width = dimensions.Width * 0.5f,
				};

				double newValue = MathUtils.Clamp01((Main.MenuUI.MousePosition.X - dragDimensions.X) / dragDimensions.Width);

				if (newValue != Value) {
					Value = newValue;

					float time = (float)TimeSystem.GlobalStopwatch.Elapsed.TotalSeconds;

					if (soundCooldownEndTime < time) {
						SoundEngine.PlaySound(soundDrag);

						soundCooldownEndTime = time + 1f / 15f;
					}
				}

				UpdateState();
			}
		}
	}

	private void UpdateState()
	{
		float percent = MathHelper.Lerp(0.0f, 0.5f, (float)Value);
		string valueString = Value.ToString("0.00");
		var valueStringSize = FontAssets.MouseText.Value.MeasureString(valueString);

		text.HAlign = 0.0f;
		text.TextOriginX = 0.0f;
		text.Left = StyleDimension.FromPixelsAndPercent(MathF.Floor(valueStringSize.X * 0.5f), percent);
		statePanel.Left = StyleDimension.FromPixelsAndPercent(0f, percent);

		text.SetText(valueString, 1.0f, false);

		Recalculate();
	}
}

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;
using TerrariaOverhaul.Core.Interface;
using TerrariaOverhaul.Core.Time;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.ConfigurationScreen;

public class RangeElement : UIElement, IConfigEntryController
{
	private readonly UIElement container;
	private readonly UIPanel background;
	private readonly UIImageButton statePanel;
	private readonly EditableUIText text;
	private bool dragging;
	private float soundCooldownEndTime;
	private string lastTextContents = string.Empty;
	private bool skipTextUpdates;

	public double MinValue { get; set; }
	public double MaxValue { get; set; }
	public double Position { get; set; }

	public double Value {
		get => MathUtils.Lerp(MinValue, MaxValue, Position);
		set => Position = MathUtils.InverseLerp(value, MinValue, MaxValue);
	}

	object? IConfigEntryController.Value {
		get => Value;
		set {
			Value = Convert.ToDouble(value!);
			UpdateState();
		}
	}

	public event Action? OnModified;

	public RangeElement() : this(0.0, 1.0) { }

	public RangeElement(double minValue, double maxValue)
	{
		MinValue = minValue;
		MaxValue = maxValue;
		MaxWidth = Width = StyleDimension.FromPixels(250f);
		MaxHeight = Height = StyleDimension.FromPercent(1.0f);

		container = this.AddElement(new UIElement().With(e => {
			e.MaxWidth = e.Width = StyleDimension.Fill;
			e.MaxHeight = e.Height = StyleDimension.Fill;

			e.SetPadding(4f);
			e.PaddingLeft = e.PaddingRight = 8f;
		}));

		background = container.AddElement(new UIPanel().With(e => {
			e.BorderColor = CommonColors.OuterPanelMedium.Border.Normal;
			e.BackgroundColor = CommonColors.OuterPanelMedium.Border.Normal;

			e.HAlign = 1f;
			e.VAlign = 0.5f;
			e.MaxWidth = e.Width = StyleDimension.FromPixels(120f);
			e.MaxHeight = e.Height = StyleDimension.FromPixelsAndPercent(-10f, 1.0f);

			e.SetPadding(0f);
		}));

		var anchorTextureDefault = ModContent.Request<Texture2D>($"{nameof(TerrariaOverhaul)}/Assets/Textures/UI/Config/RangeElementAnchor").EnsureLoaded();
		var anchorTextureHover = ModContent.Request<Texture2D>($"{nameof(TerrariaOverhaul)}/Assets/Textures/UI/Config/RangeElementAnchorHover").EnsureLoaded();

		statePanel = background.AddElement(new UIImageButton(anchorTextureDefault)).With(e => {
			e.SetVisibility(1f, 1f);
			e.SetHoverImage(anchorTextureHover);
		});

		text = container.AddElement(new EditableUIText("0.00").With(t => {
			t.MaxTextInputLength = 4;

			t.Width = StyleDimension.FromPixels(55f);
			t.HAlign = 1f;
			t.VAlign = 0.5f;
			t.Left = StyleDimension.FromPixels(-124f);
		}));

		UpdateState();
	}

	public override void LeftMouseDown(UIMouseEvent evt)
	{
		base.LeftMouseDown(evt);

		if (evt.Target != background && evt.Target != statePanel) {
			return;
		}

		SoundEngine.PlaySound(SoundID.MenuTick with { Identifier = "StartStop", Pitch = 0.1f });

		dragging = true;
	}

	public override void ScrollWheel(UIScrollWheelEvent evt)
	{
		base.ScrollWheel(evt);

		int scrollDirection = Math.Sign(evt.ScrollWheelValue);

		Value = MathUtils.Clamp(Value + scrollDirection * 0.01, MinValue, MaxValue);

		UpdateState();
		OnModified?.Invoke();
	}

	public override void Update(GameTime gameTime)
	{
		base.Update(gameTime);

		if (text.TextContent != lastTextContents) {
			if (double.TryParse(text.TextContent, out double value)) {
				Value = MathUtils.Clamp(value, MinValue, MaxValue);

				try {
					skipTextUpdates = true;
					OnModified?.Invoke();
					UpdateState();
				}
				finally {
					skipTextUpdates = false;
				}

				lastTextContents = text.TextContent;
			} else {
				text.SetText(lastTextContents!);
			}
		}

		if (dragging) {
			var soundDrag = SoundID.MenuTick with { Identifier = "Drag", Volume = 0.6f, Pitch = 0.025f, PitchVariance = 0.1f };
			var soundStop = SoundID.MenuTick with { Identifier = "StartStop", Pitch = -0.15f, PitchVariance = 0.03f };

			if (!Main.mouseLeft) {
				dragging = false;
				OnModified?.Invoke();
				SoundEngine.PlaySound(soundStop);
			} else {
				var dimensions = background.GetInnerDimensions();

				var dragDimensions = dimensions with {
					X = dimensions.X + (-2f * statePanel.HAlign + 1f) * 11f,
					Width = dimensions.Width,
				};

				double newPosition = MathUtils.Clamp01((Main.MenuUI.MousePosition.X - dragDimensions.X) / dragDimensions.Width);

				if (newPosition != Position) {
					Position = newPosition;

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
		string valueString = Value.ToString("0.00");

		statePanel.HAlign = (float)Position;

		if (!skipTextUpdates) {
			text.SetText(valueString);
		}

		Recalculate();
	}
}

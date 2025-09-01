// Copyright (c) 2020-2025 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;
using TerrariaOverhaul.Core.Interface;
using TerrariaOverhaul.Utilities.Terraria;

namespace TerrariaOverhaul.Common.ConfigurationScreen;

internal class CardPanel : FancyUIPanel
{
	private static Asset<Texture2D>? defaultBorderTexture;

	private static Asset<Texture2D> DefaultBorderTexture
		=> defaultBorderTexture ??= ModContent.Request<Texture2D>($"{nameof(TerrariaOverhaul)}/Assets/Textures/UI/Config/ThumbnailBorder").EnsureLoaded();

	public LocalizedText TitleText { get; set; }
	public object? UserObject { get; set; }

	// Elements
	public UIElement ThumbnailContainer { get; }
	public UIElement Thumbnail { get; private set; } = null!;
	public UIImage ThumbnailBorder { get; }
	public UIElement TitleContainer { get; }
	public UIElement TitleConstraint { get; }
	public ScrollingUIText Title { get; }

	public CardPanel(LocalizedText title, UIElement? thumbnail = null, Asset<Texture2D>? borderTexture = null) : base()
	{
		TitleText = title;
		borderTexture ??= DefaultBorderTexture;

		// Self

		Width = StyleDimension.FromPixels(135f);
		Height = StyleDimension.FromPixels(165f);

		Colors.CopyFrom(CommonColors.InnerPanelMediumDynamic);
		
		SoundPlayback.HoverSound = SoundID.MenuTick;

		SetPadding(0f);

		// Thumbnail

		ThumbnailContainer = this.AddElement(new UIElement().With(e => {
			e.Width = StyleDimension.FromPixels(112);
			e.Height = StyleDimension.FromPixels(112f);
			e.Top = StyleDimension.FromPixels(12f);
			e.HAlign = 0.5f;
		}));

		ThumbnailBorder = new UIImage(borderTexture).With(e => {
			e.ScaleToFit = true;
			e.Width = StyleDimension.Fill;
			e.Height = StyleDimension.Fill;
		});

		SetThumbnail(thumbnail);

		// Title

		TitleContainer = this.AddElement(new UIElement().With(e => {
			e.Width = StyleDimension.FromPixelsAndPercent(-12f, 1f);
			e.Height = StyleDimension.FromPixels(45f);
			e.HAlign = 0.5f;
			e.VAlign = 1f;
		}));

		TitleConstraint = TitleContainer.AddElement(new UIElement().With(e => {
			e.Width = StyleDimension.Fill;
			e.Height = StyleDimension.Fill;
			e.OverflowHidden = true;
		}));

		Title = TitleConstraint.AddElement(new ScrollingUIText(title).With(e => {
			e.HAlign = 0.5f;
			e.VAlign = 0.5f;
			e.ScrollStopAssistElement = this;
		}));

		if (Title.GetOuterDimensions().Width > 100f && Title.GetOuterDimensions().Width < 150f) {
			Title.SetText(title, 0.8f, false);
			Title.NoScroll = true;
		}
	}

	public void SetThumbnail(UIElement? thumbnail)
	{
		thumbnail ??= new UIElement();
		thumbnail.Width = StyleDimension.Fill;
		thumbnail.Height = StyleDimension.Fill;

		if (thumbnail is UIImage image) {
			image.ScaleToFit = true;
		} else if (thumbnail is UIVideo video) {
			video.ScaleToFit = true;
		}

		Thumbnail?.Remove();
		ThumbnailBorder.Remove();

		thumbnail.Append(ThumbnailBorder);
		ThumbnailContainer.Append(thumbnail);

		Thumbnail = thumbnail;
	}
}

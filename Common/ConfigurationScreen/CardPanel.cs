﻿using System;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using ReLogic.Content;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;
using TerrariaOverhaul.Core.Interface;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.ConfigurationScreen;

public class CardPanel : FancyUIPanel
{
	private static Asset<Texture2D>? defaultBorderTexture;

	private static Asset<Texture2D> DefaultBorderTexture
		=> defaultBorderTexture ??= ModContent.Request<Texture2D>($"{nameof(TerrariaOverhaul)}/Assets/Textures/UI/Config/ThumbnailBorder").EnsureLoaded();

	public LocalizedText TitleText { get; set; }
	public object? UserObject { get; set; }

	// Elements
	public UIElement ThumbnailContainer { get; }
	public UIElement Thumbnail { get; }
	public UIImage ThumbnailBorder { get; }
	public UIElement TitleContainer { get; }
	public UIElement TitleConstraint { get; }
	public ScrollingUIText Title { get; }

	public CardPanel(LocalizedText title, Asset<Texture2D> thumbnailTexture, Asset<Texture2D>? borderTexture = null)
		: this(title, (object)thumbnailTexture, borderTexture) { }

	public CardPanel(LocalizedText title, Asset<Video> thumbnailVideo, Asset<Texture2D>? borderTexture = null)
		: this(title, (object)thumbnailVideo, borderTexture) { }

	private CardPanel(LocalizedText title, object thumbnailAsset, Asset<Texture2D>? borderTexture = null) : base()
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

		if (thumbnailAsset is Asset<Texture2D> thumbnailTexture) {
			Thumbnail = ThumbnailContainer.AddElement(new UIImage(thumbnailTexture).With(e => {
				e.ScaleToFit = true;
			}));
		} else if (thumbnailAsset is Asset<Video> thumbnailVideo) {
			Thumbnail = ThumbnailContainer.AddElement(new UIVideo(thumbnailVideo).With(e => {
				e.ScaleToFit = true;
			}));
		} else {
			throw new InvalidOperationException();
		}

		Thumbnail.With(e => {
			e.Width = StyleDimension.Fill;
			e.Height = StyleDimension.Fill;
		});

		ThumbnailBorder = Thumbnail.AddElement(new UIImage(borderTexture).With(e => {
			e.ScaleToFit = true;
			e.Width = StyleDimension.Fill;
			e.Height = StyleDimension.Fill;
		}));

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
}

using System;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.UI.Elements;
using Terraria.UI;
using TerrariaOverhaul.Core.Configuration;
using TerrariaOverhaul.Core.Interface;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.ConfigurationScreen;

public class SettingsPanel : UIElement
{
	private const string BasePath = $"{nameof(TerrariaOverhaul)}/Assets/Textures/UI/Config";

	private static Asset<Texture2D>? iconLockedTexture;
	private static Asset<Texture2D>? unselectedIconBorderTexture;
	private static Asset<Texture2D>[]? backgroundTextures;

	private static Asset<Texture2D> UnknownOptionTexture
		=> iconLockedTexture ??= ModContent.Request<Texture2D>($"{BasePath}/UnknownOption").EnsureLoaded();

	private static Asset<Texture2D> UnselectedIconBorderTexture
		=> unselectedIconBorderTexture ??= ModContent.Request<Texture2D>($"{BasePath}/UnselectedIconBorder").EnsureLoaded();

	private static Asset<Texture2D>[] BackgroundTextures
		=> backgroundTextures ??= Enumerable.Range(1, 7).Select(i => ModContent.Request<Texture2D>($"{BasePath}/Background{i}").EnsureLoaded()).ToArray();

	// Rows
	public UIGrid OptionRowsGrid { get; }
	public UIElement OptionRowsContainer { get; }
	public FancyUIPanel OptionRowsGridContainer { get; }
	public UIScrollbar OptionRowsScrollbar { get; }
	// Bottom Panel
	public FancyUIPanel BottomPanel { get; }
	// Bottom Panel - Description
	public UIText DescriptionText { get; }
	// Bottom Panel - Icon
	public UIElement OptionIconContainer { get; }
	public UIImage UnselectedIconBorder { get; }
	public UIConfigIcon UnselectedIconImage { get; }

	public SettingsPanel() : base()
	{
		// Self

		Width = StyleDimension.Fill;
		Height = StyleDimension.Fill;

		// Main

		OptionRowsContainer = this.AddElement(new UIElement().With(e => {
			e.Width = StyleDimension.Fill;
			e.Height = StyleDimension.FromPercent(0.7f);
		}));

		OptionRowsGridContainer = OptionRowsContainer.AddElement(new FancyUIPanel().With(e => {
			e.Width = StyleDimension.FromPixelsAndPercent(-32f, 1f);
			e.Height = StyleDimension.Fill;

			e.Colors.CopyFrom(CommonColors.OuterPanelMedium);
		}));

		OptionRowsGrid = OptionRowsGridContainer.AddElement(new UIGrid().With(e => {
			e.Width = StyleDimension.Fill;
			e.Height = StyleDimension.Fill;
			e.ListPadding = 6f;
		}));

		OptionRowsScrollbar = OptionRowsContainer.AddElement(new UIScrollbar().With(e => {
			e.HAlign = 1f;
			e.VAlign = 0.5f;
			e.Height = StyleDimension.FromPixelsAndPercent(-8f, 1f);

			OptionRowsGrid.SetScrollbar(e);
			OptionRowsGrid.AddComponent(new ScrollbarListenerUIComponent { Scrollbar = e, });
		}));

		// Bottom panel

		BottomPanel = this.AddElement(new FancyUIPanel().With(e => {
			e.Width = StyleDimension.Fill;
			e.Height = StyleDimension.FromPixelsAndPercent(-12f, 0.3f);
			e.HAlign = 0.5f;
			e.VAlign = 1f;

			e.Colors.CopyFrom(CommonColors.OuterPanelBright);
		}));

		// Bottom panel - Description

		DescriptionText = BottomPanel.AddElement(new UIText(LocalizedText.Empty, textScale: 0.9f).With(e => {
			e.IsWrapped = true;
			(e.HAlign, e.VAlign) = (0.0f, 0.0f);
			(e.PaddingLeft, e.PaddingTop) = (116f, 8f);
			(e.TextOriginX, e.TextOriginY) = (0.0f, 0.0f);
			e.MaxWidth = e.Width = new StyleDimension(4f, 1f);
		}));

		// Bottom Panel - Icon

		OptionIconContainer = BottomPanel.AddElement(new UIElement().With(e => {
			e.Height = StyleDimension.FromPixels(112f);
			e.Width = StyleDimension.FromPixels(112f);
			e.VAlign = 0.5f;
		}));

		UnselectedIconImage = OptionIconContainer.AddElement(new UIConfigIcon(UnknownOptionTexture, BackgroundTextures[0]).With(e => {
			e.ResolutionOverride = UnknownOptionTexture.Value.Size();
			e.MaxWidth = e.Width = StyleDimension.FromPixelsAndPercent(-6f, 1.0f);
			e.MaxHeight = e.Height = StyleDimension.FromPixelsAndPercent(-6f, 1.0f);

			e.HAlign = 0.5f;
			e.VAlign = 0.5f;
		}));

		UnselectedIconBorder = OptionIconContainer.AddElement(new UIImage(UnselectedIconBorderTexture).With(e => {
			e.HAlign = 0.5f;
			e.VAlign = 0.5f;
		}));

		// Final

		ResetDescription();
	}

	public void AddOption(IConfigEntry configEntry)
	{
		OptionRowsGrid.AddElement(new ConfigEntryElement(configEntry).With(e => {
			e.CollisionExtents = new(3, 3, 3, 4);

			e.OnMouseOver += (_, element) => UpdateDescription((ConfigEntryElement)element);
			e.OnMouseOut += (_, _) => ResetDescription();
		}));
	}

	private void UpdateDescription(ConfigEntryElement element)
	{
		// Text
		var description = element.Description;

		DescriptionText.SetText(description, 1.0f, false);
		DescriptionText.Recalculate();

		var textDimensions = DescriptionText.GetDimensions();
		var panelDimensions = BottomPanel.GetDimensions();

		if (textDimensions.Height >= panelDimensions.Height) {
			float scale = Math.Max(0.75f, panelDimensions.Height / textDimensions.Height);

			DescriptionText.SetText(description, scale, false);
		}

		// Icon
		if (element.IconTexture != null) {
			UnselectedIconImage.ForegroundTexture = element.IconTexture;
		}

		UnselectedIconImage.BackgroundTexture = BackgroundTextures[Math.Abs(element.ConfigEntry.Name.GetHashCode()) % BackgroundTextures.Length];

		Recalculate();
	}

	private void ResetDescription()
	{
		// Text
		var descriptionTip = Language.GetText($"Mods.{nameof(TerrariaOverhaul)}.Configuration.HoverForDescriptionTip");

		DescriptionText.SetText($"[c/{Color.LightGoldenrodYellow.ToHexRGB()}:{descriptionTip}]", 1.0f, false);

		// Icon
		UnselectedIconImage.ForegroundTexture = UnknownOptionTexture;
		UnselectedIconImage.BackgroundTexture = BackgroundTextures[Main.rand.Next(BackgroundTextures.Length)];

		Recalculate();
	}
}

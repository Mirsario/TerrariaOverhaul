using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
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
	private static Asset<Texture2D>? iconLockedTexture;
	private static Asset<Texture2D>? unselectedIconBorderTexture;

	private static Asset<Texture2D> IconLockedTexture
		=> iconLockedTexture ??= ModContent.Request<Texture2D>("Terraria/Images/UI/Bestiary/Icon_Locked").EnsureLoaded();

	private static Asset<Texture2D> UnselectedIconBorderTexture
		=> unselectedIconBorderTexture ??= ModContent.Request<Texture2D>($"{nameof(TerrariaOverhaul)}/Assets/Textures/UI/Config/UnselectedIconBorder").EnsureLoaded();

	// Rows
	public UIGrid OptionRowsGrid { get; }
	public UIElement OptionRowsContainer { get; }
	public UIPanel OptionRowsGridContainer { get; }
	public UIScrollbar OptionRowsScrollbar { get; }
	// Bottom Panel
	public UIPanel BottomPanel { get; }
	// Bottom Panel - Description
	public UIText DescriptionText { get; }
	// Bottom Panel - Icon
	public UIElement OptionIconContainer { get; }
	public UIImage UnselectedIconBorder { get; }
	public UIImage UnselectedIconImage { get; }

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

		OptionRowsGridContainer = OptionRowsContainer.AddElement(new UIPanel().With(e => {
			e.Width = StyleDimension.FromPixelsAndPercent(-32f, 1f);
			e.Height = StyleDimension.Fill;
			e.BackgroundColor = new Color(54, 68, 128);
			e.BorderColor = new Color(54, 68, 128);
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
		}));

		// Bottom panel

		BottomPanel = this.AddElement(new UIPanel().With(e => {
			e.Width = StyleDimension.Fill;
			e.Height = StyleDimension.FromPixelsAndPercent(-12f, 0.3f);
			e.HAlign = 0.5f;
			e.VAlign = 1f;
			e.BackgroundColor = new Color(73, 94, 171);
			e.BorderColor = new Color(42, 54, 99);
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

		UnselectedIconBorder = OptionIconContainer.AddElement(new UIImage(UnselectedIconBorderTexture).With(e => {
			e.HAlign = 0.5f;
			e.VAlign = 0.5f;
		}));

		UnselectedIconImage = OptionIconContainer.AddElement(new UIImage(IconLockedTexture).With(e => {
			e.HAlign = 0.5f;
			e.VAlign = 0.5f;
		}));
	}

	public void AddOption(IConfigEntry configEntry)
	{
		string entryName = configEntry.Name;
		var localizedName = Language.GetText($"Mods.{nameof(TerrariaOverhaul)}.Configuration.{configEntry.Category}.{entryName}.DisplayName");
		var localizedDescription = Language.GetText($"Mods.{nameof(TerrariaOverhaul)}.Configuration.{configEntry.Category}.{entryName}.Description");

		var panel = OptionRowsGrid.AddElement(new UIPanel().With(e => {
			e.Width = StyleDimension.Fill;
			e.Height = StyleDimension.FromPixels(40f);
			e.BackgroundColor = new Color(73, 94, 171);
			e.BorderColor = new Color(42, 54, 99);

			e.OnMouseOver += (_, _) => UpdateDescription(localizedDescription);
			e.OnMouseOut += (_, _) => ResetDescription();

			var text = e.AddElement(new ScrollingUIText(localizedName).With(uiText => {

			}));
		}));
	}

	private void UpdateDescription(LocalizedText text)
	{
		DescriptionText.SetText(text, 1.0f, false);

		DescriptionText.Recalculate();

		var textDimensions = DescriptionText.GetOuterDimensions();
		var panelDimensions = BottomPanel.GetOuterDimensions();

		if (textDimensions.Height >= panelDimensions.Height) {
			float difference = textDimensions.Height - panelDimensions.Height;
			float scale = 1f - (MathF.Ceiling(difference / 10f) * 0.05f);

			DescriptionText.SetText(text, scale, false);
		}
	}

	private void ResetDescription()
	{
		DescriptionText.SetText(LocalizedText.Empty);
	}
}

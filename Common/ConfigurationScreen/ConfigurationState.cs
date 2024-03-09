using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.UI.Elements;
using Terraria.UI;
using TerrariaOverhaul.Core.Configuration;
using TerrariaOverhaul.Core.Interface;

namespace TerrariaOverhaul.Common.ConfigurationScreen;

public sealed class ConfigurationState : UIState
{
	private static ConfigurationState? instance;

	public static ConfigurationState Instance => instance ??= new();

	private bool inCategoryMenu = true;

	public BetterSearchBar SearchBar { get; set; } = null!;
	// Main
	public FancyUIPanel MainPanel { get; private set; } = null!;
	public UIElement ContentArea { get; private set; } = null!;
	public UITextPanel<LocalizedText> ExitButton { get; private set; } = null!;
	public UIElement ContentContainer { get; private set; } = null!;
	public UIElement PanelGridContainer { get; private set; } = null!;
	public UIGrid PanelGrid { get; private set; } = null!;

	public override void OnInitialize()
	{
		// Main Elements

		ContentArea = this.AddElement(new UIElement().With(e => {
			e.Width = StyleDimension.FromPixels(800f);
			e.Top = StyleDimension.FromPixels(220f);
			e.Height = StyleDimension.FromPixelsAndPercent(-220f, 1f);
			e.HAlign = 0.5f;
		}));

		ExitButton = ContentArea.AddElement(new UITextPanel<LocalizedText>(Language.GetText("UI.Back"), 0.7f, large: true).With(e => {
			e.Width = StyleDimension.FromPixelsAndPercent(-10f, 0.5f);
			e.Height = StyleDimension.FromPixels(50f);
			e.VAlign = 1f;
			e.HAlign = 0.5f;
			e.Top = StyleDimension.FromPixels(-25f);

			e.AddComponent(new DynamicColorsUIComponent().CopyFrom(
				CommonColors.OuterPanelMediumDynamic
			));

			e.AddComponent(new SoundPlaybackUIComponent {
				HoverSound = SoundID.MenuTick,
			});

			e.OnLeftClick += (_, _) => BackButtonLogic();

			e.SetSnapPoint("ExitButton", 0);
		}));

		MainPanel = ContentArea.AddElement(new FancyUIPanel().With(e => {
			e.Width = StyleDimension.Fill;
			e.Height = StyleDimension.FromPixelsAndPercent(-90f, 1f);

			e.Colors.CopyFrom(CommonColors.OuterPanelDark);

			e.SetPadding(0f);
		}));

		SearchBar = MainPanel.AddElement(new BetterSearchBar().With(e => {
			e.Width = StyleDimension.Fill;
			e.Top = StyleDimension.FromPixels(10f);
			e.VAlign = 0f;

			e.TextInput.OnContentsChanged += HighlightPanelsViaSearchString;
		}));

		ContentContainer = MainPanel.AddElement(new UIElement().With(e => {
			e.Width = StyleDimension.Fill;
			e.Height = StyleDimension.FromPixelsAndPercent(-48f, 1f);
			e.Top = StyleDimension.FromPixels(48f);
			e.PaddingLeft = 15f;
			e.PaddingRight = 15f;
			e.PaddingBottom = 15f;
		}));

		// Panel Grid

		PanelGridContainer = ContentContainer.AddElement(new UIElement().With(e => {
			e.Width = StyleDimension.Fill;
			e.Height = StyleDimension.Fill;
		}));

		PanelGrid = PanelGridContainer.AddElement(new UIGrid().With(e => {
			e.Width = StyleDimension.FromPixelsAndPercent(-20, 1f);
			e.Height = StyleDimension.Fill;
			e.ListPadding = 15f;
			e.PaddingRight = 15f;
		}));

		var panelGridScrollbar = PanelGridContainer.AddElement(new UIScrollbar().With(e => {
			e.HAlign = 1f;
			e.VAlign = 0.5f;
			e.Height = StyleDimension.FromPixelsAndPercent(-16f, 1f);

			PanelGrid.SetScrollbar(e);
			PanelGrid.AddComponent(new ScrollbarListenerUIComponent { Scrollbar = e, });
		}));

		var configCategories = ConfigSystem.CategoriesByName.Keys.OrderBy(s => s);
		var thumbnailPlaceholder = ModContent.Request<Texture2D>($"{nameof(TerrariaOverhaul)}/Assets/Textures/UI/Config/NoPreview");

		foreach (string category in configCategories) {
			var localizedCategoryName = Language.GetText($"Mods.{nameof(TerrariaOverhaul)}.Configuration.{category}.DisplayName");

			ConfigMediaLookup.TryGetMedia(category, "Category", out var mediaResult, ConfigMediaKind.Image | ConfigMediaKind.Video);

			var cardPanel = mediaResult.mediaAsset switch {
				Asset<Video> video => new CardPanel(localizedCategoryName, video),
				Asset<Texture2D> image => new CardPanel(localizedCategoryName, image),
				_ => new CardPanel(localizedCategoryName, thumbnailPlaceholder),
			};

			PanelGrid.Add(cardPanel);

			cardPanel.OnLeftClick += (_, _) => SwitchToCategorySettings(category);
		}
	}

	private void SwitchToCategorySettings(string category)
	{
		SoundEngine.PlaySound(in SoundID.MenuOpen);

		PanelGridContainer.Remove();

		ContentContainer.AddElement(new SettingsPanel().With(e => {
			var categoryData = ConfigSystem.CategoriesByName[category];

			foreach (var configEntry in categoryData.EntriesByName.Values) {
				e.AddOption(configEntry);
			}
		}));

		inCategoryMenu = false;

		HighlightPanelsViaSearchString(SearchBar.SearchString);
	}

	private void BackButtonLogic()
	{
		SoundEngine.PlaySound(SoundID.MenuClose);

		if (inCategoryMenu) {
			Main.menuMode = MenuID.Title;
		} else {
			ContentContainer.RemoveAllChildren();
			ContentContainer.Append(PanelGridContainer);

			inCategoryMenu = true;

			HighlightPanelsViaSearchString(SearchBar.SearchString);
		}
	}

	private void HighlightPanelsViaSearchString(string? searchString)
	{
		IEnumerable<UIElement> panels;

		if (inCategoryMenu) {
			panels = PanelGrid.Children.First().Children;
		} else {
			panels = ContentContainer.GetFirstChildAt<UIElement>(5, e => e is UIGrid)?.Children?.FirstOrDefault()?.Children 
				?? Enumerable.Empty<UIElement>();
		}

		// Searchbar is empty, reset all panels
		if (string.IsNullOrEmpty(searchString)) {
			foreach (FancyUIPanel panel in panels.Cast<FancyUIPanel>()) {
				panel.Colors.OverrideBorderColor = null;
			}

			return;
		}

		var comparison = StringComparison.InvariantCultureIgnoreCase;

		bool TextContains(LocalizedText text, string searchString)
			=> text.Value.Contains(searchString, comparison);

		if (inCategoryMenu) {
			int index = -1;

			foreach (var panel in panels.Cast<CardPanel>()) {
				index++;
				string categoryId = ConfigSystem.CategoriesByName.Keys.OrderBy(s => s).ElementAt(index);

				if (!ConfigSystem.CategoriesByName.TryGetValue(categoryId, out var category)) {
					continue;
				}

				var categoryEntries = category.EntriesByName.Values;
				bool categoryNameHasSearchString = TextContains(panel.TitleText, searchString);
				bool categoryEntryNameHasSearchString = categoryEntries.Any(i => TextContains(Language.GetText($"Mods.{nameof(TerrariaOverhaul)}.Configuration.{category}.{i.Name}.DisplayName"), searchString));
				bool either = categoryNameHasSearchString || categoryEntryNameHasSearchString;

				// Change to whatever color the border should be when highlighted. Note: theres also OverrideBackgroundColor
				panel.Colors.OverrideBorderColor = either ? CommonColors.DefaultHover : null;
			}
		} else {
			foreach (var entry in panels.Cast<ConfigEntryElement>()) {
				bool isMatched = TextContains(entry.DisplayName, searchString);

				entry.Colors.OverrideBorderColor = isMatched ? CommonColors.DefaultHover : null;
			}
		}
	}

	public override void LeftClick(UIMouseEvent evt)
	{
		if (SearchBar.IsFocused) {
			SearchBar.ToggleFocus();
		}

		base.LeftClick(evt);
	}

	public override void Update(GameTime gameTime)
	{
		base.Update(gameTime);

		if (Main.keyState.IsKeyDown(Keys.Escape) && !Main.oldKeyState.IsKeyDown(Keys.Escape)) {
			if (SearchBar.IsFocused) {
				SearchBar.IsFocused = false;
			} else {
				BackButtonLogic();
			}
		}
	}
}

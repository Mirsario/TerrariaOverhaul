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

	private readonly List<CardPanel> categoryPanels = new();
	private SettingsPanel? currentSettingsPanel;

	public BetterSearchBar SearchBar { get; set; } = null!;
	// Main
	public FancyUIPanel MainPanel { get; private set; } = null!;
	public UIElement ContentArea { get; private set; } = null!;
	public UITextPanel<LocalizedText> ExitButton { get; private set; } = null!;
	public UIElement ContentContainer { get; private set; } = null!;
	public UIElement PanelGridContainer { get; private set; } = null!;
	public UIGrid PanelGrid { get; private set; } = null!;
	public UIScrollbar PanelGridScrollbar { get; private set; } = null!;

	public bool OnMainScreen => currentSettingsPanel == null;

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

			e.OnSearchStringUpdated += RefreshState;
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

		PanelGridScrollbar = PanelGridContainer.AddElement(new UIScrollbar().With(e => {
			e.HAlign = 1f;
			e.VAlign = 0.5f;
			e.Height = StyleDimension.FromPixelsAndPercent(-16f, 1f);

			PanelGrid.SetScrollbar(e);
			PanelGrid.AddComponent(new ScrollbarListenerUIComponent { Scrollbar = e, });
		}));

		InitializeCategoryPanels();
		RefreshState();
	}

	private void InitializeCategoryPanels()
	{
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

			cardPanel.UserObject = category;
			cardPanel.OnLeftClick += (_, _) => {
				SoundEngine.PlaySound(in SoundID.MenuOpen);
				SetCategoryScreen(category);
			};

			categoryPanels.Add(cardPanel);
		}
	}

	private void SetCategoryScreen(string? category)
	{
		if (category == null) {
			currentSettingsPanel = null;

			ContentContainer.RemoveAllChildren();
			ContentContainer.Append(PanelGridContainer);
		} else {
			currentSettingsPanel = new SettingsPanel().With(e => {
				var categoryData = ConfigSystem.CategoriesByName[category!];

				foreach (var configEntry in categoryData.EntriesByName.Values) {
					e.AddOption(configEntry);
				}
			});

			PanelGridContainer.Remove();
			ContentContainer.AddElement(currentSettingsPanel);
		}

		RefreshState();
	}

	private void BackButtonLogic()
	{
		SoundEngine.PlaySound(SoundID.MenuClose);

		if (OnMainScreen) {
			Main.menuMode = MenuID.Title;
		} else {
			SetCategoryScreen(null);
		}
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

	public bool IsElementMatchedBySearch(UIElement element)
	{
		var comparison = StringComparison.InvariantCultureIgnoreCase;
		string searchString = SearchBar.SearchString;

		if (searchString.Length == 0) {
			return true;
		}

		bool StringMatches(string text)
			=> text.Contains(searchString, comparison);

		bool TextMatches(LocalizedText text)
			=> text.Value.Contains(searchString, comparison);

		if (element is ConfigEntryElement entryElement) {
			return StringMatches(entryElement.ConfigEntry.Name)
				|| TextMatches(entryElement.DisplayName);
		}

		if (element is CardPanel { UserObject: string categoryName } categoryPanel
		&& ConfigSystem.CategoriesByName.TryGetValue(categoryName, out var categoryData)) {
			if (StringMatches(categoryName)
			|| TextMatches(categoryPanel.TitleText)) {
				return true;
			}

			foreach (var entry in categoryData.EntriesByName.Values) {
				if (StringMatches(entry.Name)) {
					return true;
				}

				if (TextMatches(Language.GetText($"Mods.{nameof(TerrariaOverhaul)}.Configuration.{categoryName}.{entry.Name}.DisplayName"))) {
					return true;
				}
			}

			return false;
		}

		return false;
	}

	private void RefreshState()
	{
		bool enableHighlighting = SearchBar.SearchString.Length >= 1;

		if (OnMainScreen) {
			RefreshCategories(enableHighlighting);
		} else {
			RefreshOptions(enableHighlighting);
		}

		Recalculate();
	}

	private void RefreshCategories(bool enableHighlighting)
	{
		static void SetHighlight(CardPanel cardPanel, bool? highlight)
		{
			cardPanel.Colors.CopyFrom(CommonColors.InnerPanelMediumDynamic);

			if (highlight == true) {
				cardPanel.Colors.Background = CommonColors.InnerPanelBright.Background;
				cardPanel.Colors.Border.Hover = CommonColors.DefaultHover;
				cardPanel.Colors.Border.Normal = Color.Lerp(cardPanel.Colors.Border.Normal, CommonColors.DefaultHover, 0.5f);
			} else if (highlight == false) {
				cardPanel.Colors.Background.Normal = CommonColors.OuterPanelDark.Background.Normal;
			}
		}

		var container = PanelGrid;
		var panels = categoryPanels.Select(p => (p, isMatched: IsElementMatchedBySearch(p))).OrderByDescending(t => t.isMatched);

		container.Clear();

		foreach (var (cardPanel, isMatched) in panels) {
			SetHighlight(cardPanel, enableHighlighting ? isMatched : null);
			container.Add(cardPanel);
		}

		container.Recalculate();
	}

	private void RefreshOptions(bool enableHighlighting)
	{
		if (currentSettingsPanel is not SettingsPanel settingsPanel) {
			return;
		}

		static void SetHighlight(ConfigEntryElement element, bool? highlight)
		{
			element.Colors.CopyFrom(CommonColors.InnerPanelDarkDynamic);

			if (highlight == true) {
				element.Colors.Background = CommonColors.InnerPanelBright.Background;
				element.Colors.Border.Hover = CommonColors.DefaultHover;
				element.Colors.Border.Normal = Color.Lerp(element.Colors.Border.Normal, CommonColors.DefaultHover, 0.5f);
			} else if (highlight == false) {
				element.Colors.Background.Normal = CommonColors.OuterPanelDark.Background.Normal;
			}
		}

		var container = settingsPanel.OptionRowsGrid;
		var entries = settingsPanel.OptionElements.Select(p => (p, isMatched: IsElementMatchedBySearch(p))).OrderByDescending(t => t.isMatched);

		container.Clear();

		foreach (var (cardPanel, isMatched) in entries) {
			SetHighlight(cardPanel, enableHighlighting ? isMatched : null);
			container.Add(cardPanel);
		}

		container.Recalculate();
	}
}

using System;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
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
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.ConfigurationScreen;

public class ConfigurationUIState : UIState
{
	// Search
	private string? searchString;
	// Etc.
	private bool clickedSearchBar;
	private bool clickedSomething;

	// Main
	public UIPanel MainPanel { get; private set; } = null!;
	public UIElement ContentArea { get; private set; } = null!;
	public UITextPanel<LocalizedText> ExitButton { get; private set; } = null!;
	public UIElement GridPage { get; private set; } = null!;
	// Search
	public UIImageButton SearchButton { get; private set; } = null!;
	public UISearchBar SearchBar { get; private set; } = null!;
	public UIPanel SearchBarPanel { get; private set; } = null!;

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

			e.OnMouseOver += FadedMouseOver;
			e.OnMouseOut += FadedMouseOut;
			e.OnMouseDown += Click_GoBack;

			e.SetSnapPoint("ExitButton", 0);
		}));

		MainPanel = ContentArea.AddElement(new UIPanel().With(e => {
			e.Width = StyleDimension.Fill;
			e.Height = StyleDimension.FromPixelsAndPercent(-90f, 1f);
			e.BackgroundColor = new Color(33, 43, 79) * 0.8f;

			e.SetPadding(0f);
		}));

		GridPage = MainPanel.AddElement(new UIElement().With(e => {
			e.Width = StyleDimension.Fill;
			e.Height = StyleDimension.Fill;
		}));

		// Search Bar

		var searchBarSection = GridPage.AddElement(new UIElement().With(e => {
			e.Width = StyleDimension.Fill;
			e.Height = StyleDimension.FromPixels(24f);
			e.Top = StyleDimension.FromPixels(12f);
			e.VAlign = 0f;

			e.SetPadding(0f);
		}));

		SearchBarPanel = searchBarSection.AddElement(new UIPanel().With(e => {
			e.Width = StyleDimension.FromPercent(0.95f);
			e.Height = StyleDimension.Fill;
			e.HAlign = 0.5f;
			e.VAlign = 0.5f;
			e.BorderColor = new Color(73, 94, 171);
			e.BackgroundColor = new Color(73, 94, 171);

			e.SetPadding(0f);
		}));

		SearchBar = SearchBarPanel.AddElement(new UISearchBar(Language.GetText("Search"), 0.8f).With(e => {
			e.Width = StyleDimension.Fill;
			e.Height = StyleDimension.Fill;
			e.HAlign = 0f;
			e.VAlign = 0.5f;

			e.OnClick += Click_SearchArea;
			e.OnContentsChanged += OnSearchContentsChanged;
			e.OnStartTakingInput += OnStartTakingInput;
			e.OnEndTakingInput += OnEndTakingInput;

			e.SetContents(null, forced: true);
		}));

		var searchCancelButton = SearchBar.AddElement(new UIImageButton(Main.Assets.Request<Texture2D>("Images/UI/SearchCancel")).With(e => {
			e.HAlign = 1f;
			e.VAlign = 0.5f;
			e.Left = StyleDimension.FromPixels(-2f);

			e.OnMouseOver += SearchCancelButton_OnMouseOver;
			e.OnClick += SearchCancelButton_OnClick;
		}));

		// Panel Grid

		var panelGridContainer = GridPage.AddElement(new UIElement().With(e => {
			e.Width = StyleDimension.Fill;
			e.Height = StyleDimension.FromPixelsAndPercent(-48f, 1f);
			e.Top = StyleDimension.FromPixels(48f);
			e.PaddingLeft = 15f;
			e.PaddingRight = 15f;
			e.PaddingBottom = 15f;
		}));

		var panelGrid = panelGridContainer.AddElement(new UIGrid().With(e => {
			e.Width = StyleDimension.FromPixelsAndPercent(-20, 1f);
			e.Height = StyleDimension.Fill;
			e.ListPadding = 15f;
			e.PaddingRight = 15f;
		}));

		var panelGridScrollbar = panelGridContainer.AddElement(new UIScrollbar().With(e => {
			e.HAlign = 1f;
			e.VAlign = 0.5f;
			e.Height = StyleDimension.FromPixelsAndPercent(-8f, 1f);

			panelGrid.SetScrollbar(e);
		}));

		string assetLocation = $"{nameof(TerrariaOverhaul)}/Assets/Textures/UI/Config";

		var thumbnailPlaceholder = ModContent.Request<Texture2D>($"{assetLocation}/NoPreview");
		var configCategories = ConfigSystem.CategoriesByName.Keys.OrderBy(s => s);

		foreach (string category in configCategories) {
			var localizedCategoryName = Language.GetText($"Mods.{nameof(TerrariaOverhaul)}.Configuration.{category}.Category.DisplayName");

			string thumbnailPath = $"{assetLocation}/{category}/Category";
			string thumbnailVideoPath = $"{thumbnailPath}Video";

			ConfigPanel configPanel;

			if (ModContent.HasAsset(thumbnailVideoPath)) {
				var thumbnailVideo = ModContent.Request<Video>(thumbnailVideoPath);

				configPanel = new ConfigPanel(localizedCategoryName, thumbnailVideo);
			} else {
				var thumbnailTexture = ModContent.HasAsset(thumbnailPath) ? ModContent.Request<Texture2D>(thumbnailPath) : thumbnailPlaceholder;

				configPanel = new ConfigPanel(localizedCategoryName, thumbnailTexture);
			}

			panelGrid.Add(configPanel);

			configPanel.OnClick += ConfigPanel_OnClick;
		}
	}

	private void ConfigPanel_OnClick(UIMouseEvent evt, UIElement listeningElement)
	{
		GridPage.Remove();

		var panel = (ConfigPanel)listeningElement;
		MainPanel.AddElement(new SettingsPanel(panel.titleText));
	}

	private void Click_GoBack(UIMouseEvent evt, UIElement listeningElement)
	{
		SoundEngine.PlaySound(SoundID.MenuClose);
		Main.menuMode = MenuID.Title;
	}

	private void FadedMouseOver(UIMouseEvent evt, UIElement listeningElement)
	{
		SoundEngine.PlaySound(SoundID.MenuTick);
		((UIPanel)evt.Target).BackgroundColor = new Color(73, 94, 171);
		((UIPanel)evt.Target).BorderColor = Colors.FancyUIFatButtonMouseOver;
	}

	private void FadedMouseOut(UIMouseEvent evt, UIElement listeningElement)
	{
		((UIPanel)evt.Target).BackgroundColor = new Color(63, 82, 151) * 0.8f;
		((UIPanel)evt.Target).BorderColor = Color.Black;
	}

	#region Search Bar Nonsense

	private void Click_SearchArea(UIMouseEvent evt, UIElement listeningElement)
	{
		if (SearchBar == null) {
			return;
		}

		if (listeningElement == SearchBar || listeningElement == SearchButton || listeningElement == SearchBarPanel) {
			SearchBar.ToggleTakingText();

			clickedSearchBar = true;
		}
	}

	private void OnSearchContentsChanged(string contents)
	{
		searchString = contents;
	}

	private void OnStartTakingInput()
	{
		SearchBarPanel.BorderColor = Main.OurFavoriteColor;
	}

	private void OnEndTakingInput()
	{
		SearchBarPanel.BorderColor = new Color(73, 94, 171);
	}

	private void OnFinishedSettingName(string name)
	{
		string contents = name.Trim();

		SearchBar.SetContents(contents);
		GoBackHere();
	}

	private void GoBackHere()
	{
		UserInterface.ActiveInstance.SetState(this);

		if (SearchBar.IsWritingText) {
			SearchBar.ToggleTakingText();
		}
	}

	private void SearchCancelButton_OnClick(UIMouseEvent evt, UIElement listeningElement)
	{
		if (SearchBar.HasContents) {
			SearchBar.SetContents(null, forced: true);
			SoundEngine.PlaySound(SoundID.MenuClose);
		} else {
			SoundEngine.PlaySound(SoundID.MenuTick);
		}

		GoBackHere();
	}

	private void SearchCancelButton_OnMouseOver(UIMouseEvent evt, UIElement listeningElement)
	{
		SoundEngine.PlaySound(SoundID.MenuTick);
	}

	public override void Click(UIMouseEvent evt)
	{
		base.Click(evt);

		clickedSomething = true;
	}

	#endregion

	public override void Update(GameTime gameTime)
	{
		if (clickedSomething && !clickedSearchBar && SearchBar.IsWritingText) {
			SearchBar.ToggleTakingText();
		}

		if (Main.keyState.IsKeyDown(Keys.Escape) && !Main.oldKeyState.IsKeyDown(Keys.Escape)) {
			if (SearchBar.IsWritingText) {
				GoBackHere();
			} else {
				SoundEngine.PlaySound(SoundID.MenuClose);
				Main.menuMode = MenuID.Title;
			}
		}
	}
}

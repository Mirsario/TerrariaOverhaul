using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader.UI.Elements;
using Terraria.UI;

namespace TerrariaOverhaul.Core.Configuration;

public class ConfigurationUIState : UIState
{
	// Elements
	private UIImageButton searchButton = null!;
	private UISearchBar searchBar = null!;
	private UIPanel searchBarPanel = null!;
	// Search
	private string? searchString;
	// Etc.
	private bool clickedSearchBar;
	private bool clickedSomething;

	public override void OnInitialize()
	{
		#region Main Elements

		var contentArea = new UIElement {
			Width = StyleDimension.FromPixels(800f),
			Top = StyleDimension.FromPixels(220f),
			Height = StyleDimension.FromPixelsAndPercent(-220f, 1f),
			HAlign = 0.5f
		};

		Append(contentArea);

		var exitButton = new UITextPanel<LocalizedText>(Language.GetText("UI.Back"), 0.7f, large: true) {
			Width = StyleDimension.FromPixelsAndPercent(-10f, 0.5f),
			Height = StyleDimension.FromPixels(50f),
			VAlign = 1f,
			HAlign = 0.5f,
			Top = StyleDimension.FromPixels(-25f)
		};

		exitButton.OnMouseOver += FadedMouseOver;
		exitButton.OnMouseOut += FadedMouseOut;
		exitButton.OnMouseDown += Click_GoBack;
		exitButton.SetSnapPoint("ExitButton", 0);
		contentArea.Append(exitButton);

		var mainPanel = new UIPanel {
			Width = StyleDimension.FromPercent(1f),
			Height = StyleDimension.FromPixelsAndPercent(-90f, 1f),
			BackgroundColor = new Color(33, 43, 79) * 0.8f
		};

		mainPanel.SetPadding(0f);
		contentArea.Append(mainPanel);

		#endregion

		#region Search Bar

		var searchBarSection = new UIElement {
			Width = StyleDimension.FromPercent(1f),
			Height = StyleDimension.FromPixels(24f),
			Top = StyleDimension.FromPixels(12f),
			VAlign = 0f,
		};

		searchBarSection.SetPadding(0f);
		mainPanel.Append(searchBarSection);

		searchBarPanel = new() {
			Width = StyleDimension.FromPercent(0.95f),
			Height = StyleDimension.FromPercent(1f),
			HAlign = 0.5f,
			VAlign = 0.5f,
			BorderColor = new Color(73, 94, 171),
			BackgroundColor = new Color(73, 94, 171)
		};

		searchBarPanel.SetPadding(0f);
		searchBarSection.Append(searchBarPanel);

		searchBar = new(Language.GetText("Search"), 0.8f) {
			Width = StyleDimension.FromPercent(1f),
			Height = StyleDimension.FromPercent(1f),
			HAlign = 0f,
			VAlign = 0.5f,
		};

		searchBar.OnClick += Click_SearchArea;
		searchBar.OnContentsChanged += OnSearchContentsChanged;
		searchBar.OnStartTakingInput += OnStartTakingInput;
		searchBar.OnEndTakingInput += OnEndTakingInput;
		searchBarPanel.Append(searchBar);
		searchBar.SetContents(null, forced: true);

		var searchCancelButton = new UIImageButton(Main.Assets.Request<Texture2D>("Images/UI/SearchCancel")) {
			HAlign = 1f,
			VAlign = 0.5f,
			Left = StyleDimension.FromPixels(-2f)
		};

		searchCancelButton.OnMouseOver += SearchCancelButton_OnMouseOver;
		searchCancelButton.OnClick += SearchCancelButton_OnClick;
		searchBar.Append(searchCancelButton);

		#endregion

		#region Panel Grid

		var panelGridContainer = new UIElement() {
			Width = StyleDimension.FromPercent(1f),
			Height = StyleDimension.FromPixelsAndPercent(-48f, 1f),
			Top = StyleDimension.FromPixels(48f),
			PaddingBottom = 15f
		};

		mainPanel.Append(panelGridContainer);

		var panelGrid = new UIGrid() {
			Width = StyleDimension.FromPercent(0.95f),
			Height = StyleDimension.FromPercent(1f),
			HAlign = 0.5f,
			ListPadding = 15f,
			PaddingLeft = 15f,
			PaddingRight = 15f
		};

		panelGridContainer.Append(panelGrid);

		var panelGridScrollbar = new UIScrollbar() /*{
			Height = StyleDimension.FromPixelsAndPercent(-8f, 1f),
			Left = StyleDimension.FromPixelsAndPercent(-2f, 1f),
			VAlign = 0.5f
		}*/;

		panelGrid.SetScrollbar(panelGridScrollbar);
		// PanelGridContainer.Append(PanelGridScrollbar);

		for (int i = 1; i <= 25; i++) {
			panelGrid.Add(new ConfigPanel(/* thumbnail image path */));
		}

		#endregion
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
		if (searchBar == null) {
			return;
		}

		if (listeningElement == searchBar || listeningElement == searchButton || listeningElement == searchBarPanel) {
			searchBar.ToggleTakingText();

			clickedSearchBar = true;
		}
	}

	private void OnSearchContentsChanged(string contents)
	{
		searchString = contents;
	}

	private void OnStartTakingInput()
	{
		searchBarPanel.BorderColor = Main.OurFavoriteColor;
	}

	private void OnEndTakingInput()
	{
		searchBarPanel.BorderColor = new Color(73, 94, 171);
	}

	private void OnFinishedSettingName(string name)
	{
		string contents = name.Trim();

		searchBar.SetContents(contents);
		GoBackHere();
	}

	private void GoBackHere()
	{
		UserInterface.ActiveInstance.SetState(this);

		if (searchBar.IsWritingText) {
			searchBar.ToggleTakingText();
		}
	}

	private void SearchCancelButton_OnClick(UIMouseEvent evt, UIElement listeningElement)
	{
		if (searchBar.HasContents) {
			searchBar.SetContents(null, forced: true);
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
		if (clickedSomething && !clickedSearchBar && searchBar.IsWritingText) {
			searchBar.ToggleTakingText();
		}

		if (Main.keyState.IsKeyDown(Keys.Escape) && !Main.oldKeyState.IsKeyDown(Keys.Escape)) {
			if (searchBar.IsWritingText) {
				GoBackHere();
			} else {
				SoundEngine.PlaySound(SoundID.MenuClose);
				Main.menuMode = MenuID.Title;
			}
		}
	}
}

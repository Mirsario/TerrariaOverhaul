using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.ModLoader.UI.Elements;
using Terraria.UI;
using Terraria;
using Terraria.Localization;
using Terraria.Audio;

namespace TerrariaOverhaul.Core.Configuration.ConfigurationUI;

public class ConfigurationUIState : UIState
{
	string SearchString;

	bool ClickedSearchBar = false;
	bool ClickedSomething = false;

	UIImageButton SearchButton;
	UISearchBar SearchBar;
	UIPanel SearchBarPanel;

	public override void OnInitialize()
	{
		#region Main Elements

		UIElement ContentArea = new() {
			Width = StyleDimension.FromPixels(800f),
			Top = StyleDimension.FromPixels(220f),
			Height = StyleDimension.FromPixelsAndPercent(-220f, 1f),
			HAlign = 0.5f
		};

		Append(ContentArea);

		UITextPanel<LocalizedText> ExitButton = new(Language.GetText("UI.Back"), 0.7f, large: true) {
			Width = StyleDimension.FromPixelsAndPercent(-10f, 0.5f),
			Height = StyleDimension.FromPixels(50f),
			VAlign = 1f,
			HAlign = 0.5f,
			Top = StyleDimension.FromPixels(-25f)
		};

		ExitButton.OnMouseOver += FadedMouseOver;
		ExitButton.OnMouseOut += FadedMouseOut;
		ExitButton.OnMouseDown += Click_GoBack;
		ExitButton.SetSnapPoint("ExitButton", 0);
		ContentArea.Append(ExitButton);

		UIPanel MainPanel = new() {
			Width = StyleDimension.FromPercent(1f),
			Height = StyleDimension.FromPixelsAndPercent(-90f, 1f),
			BackgroundColor = new Color(33, 43, 79) * 0.8f
		};

		MainPanel.SetPadding(0f);
		ContentArea.Append(MainPanel);

		#endregion

		#region Search Bar

		UIElement SearchBarSection = new() {
			Width = StyleDimension.FromPercent(1f),
			Height = StyleDimension.FromPixels(24f),
			Top = StyleDimension.FromPixels(12f),
			VAlign = 0f,
		};

		SearchBarSection.SetPadding(0f);
		MainPanel.Append(SearchBarSection);

		SearchBarPanel = new() {
			Width = StyleDimension.FromPercent(0.95f),
			Height = StyleDimension.FromPercent(1f),
			HAlign = 0.5f,
			VAlign = 0.5f,
			BorderColor = new Color(73, 94, 171),
			BackgroundColor = new Color(73, 94, 171)
		};

		SearchBarPanel.SetPadding(0f);
		SearchBarSection.Append(SearchBarPanel);

		SearchBar = new(Language.GetText("Search"), 0.8f) {
			Width = StyleDimension.FromPercent(1f),
			Height = StyleDimension.FromPercent(1f),
			HAlign = 0f,
			VAlign = 0.5f,
		};

		SearchBar.OnClick += Click_SearchArea;
		SearchBar.OnContentsChanged += OnSearchContentsChanged;
		SearchBar.OnStartTakingInput += OnStartTakingInput;
		SearchBar.OnEndTakingInput += OnEndTakingInput;
		SearchBarPanel.Append(SearchBar);
		SearchBar.SetContents(null, forced: true);

		UIImageButton SearchCancelButton = new(Main.Assets.Request<Texture2D>("Images/UI/SearchCancel")) {
			HAlign = 1f,
			VAlign = 0.5f,
			Left = StyleDimension.FromPixels(-2f)
		};

		SearchCancelButton.OnMouseOver += SearchCancelButton_OnMouseOver;
		SearchCancelButton.OnClick += SearchCancelButton_OnClick;
		SearchBar.Append(SearchCancelButton);

		#endregion

		#region Panel Grid

		UIElement PanelGridContainer = new() {
			Width = StyleDimension.FromPercent(1f),
			Height = StyleDimension.FromPixelsAndPercent(-48f, 1f),
			Top = StyleDimension.FromPixels(48f),
			PaddingBottom = 15f
		};

		MainPanel.Append(PanelGridContainer);

		UIGrid PanelGrid = new() {
			Width = StyleDimension.FromPercent(0.95f),
			Height = StyleDimension.FromPercent(1f),
			HAlign = 0.5f,
			ListPadding = 15f,
			PaddingLeft = 15f,
			PaddingRight = 15f
		};

		PanelGridContainer.Append(PanelGrid);

		UIScrollbar PanelGridScrollbar = new() /*{
			Height = StyleDimension.FromPixelsAndPercent(-8f, 1f),
			Left = StyleDimension.FromPixelsAndPercent(-2f, 1f),
			VAlign = 0.5f
		}*/;

		PanelGrid.SetScrollbar(PanelGridScrollbar);
		// PanelGridContainer.Append(PanelGridScrollbar);

		for (int i = 1; i <= 25; i++) {
			PanelGrid.Add(new ConfigPanel(/* thumbnail image path */));
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
		if (listeningElement == SearchBar || listeningElement == SearchButton || listeningElement == SearchBarPanel) {
			SearchBar.ToggleTakingText();

			ClickedSearchBar = true;
		}
	}

	private void OnSearchContentsChanged(string contents)
	{
		SearchString = contents;
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

		ClickedSomething = true;
	}

	#endregion

	public override void Update(GameTime gameTime)
	{
		if (ClickedSomething && !ClickedSearchBar && SearchBar.IsWritingText) {
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

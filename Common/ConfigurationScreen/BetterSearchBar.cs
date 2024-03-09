using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.UI;
using TerrariaOverhaul.Core.Input;
using TerrariaOverhaul.Core.Interface;

namespace TerrariaOverhaul.Common.ConfigurationScreen;

public class BetterSearchBar : UIElement
{
	public FancyUIPanel Container { get; }
	public UISearchBar TextInput { get; }
	public UIImageButton ClearTextInputButton { get; }
	public string SearchString { get; set; }

	public bool IsFocused {
		get => TextInput.IsWritingText;
		set {
			if (IsFocused != value) {
				TextInput.ToggleTakingText();
			}
		}
	}

	public event Action? OnSearchStringUpdated;

	public BetterSearchBar(string? searchString = null) : base()
	{
		SearchString = searchString ?? string.Empty;

		Height = StyleDimension.FromPixels(28f);

		SetPadding(0f);

		Container = this.AddElement(new FancyUIPanel().With(e => {
			e.Width = StyleDimension.FromPercent(0.95f);
			e.Height = StyleDimension.Fill;
			e.HAlign = 0.5f;
			e.VAlign = 0.5f;

			e.Colors.CopyFrom(CommonColors.InnerPanelBrightDynamic);

			e.SetPadding(0f);
		}));

		TextInput = Container.AddElement(new UISearchBar(Language.GetText("Search"), 0.8f).With(e => {
			e.Width = StyleDimension.Fill;
			e.Height = StyleDimension.Fill;
			e.HAlign = 0f;
			e.VAlign = 0.5f;

			e.SetContents(searchString, true);
			e.OnContentsChanged += (string obj) => {
				SoundEngine.PlaySound(SoundID.MenuTick with { Volume = 0.75f, PitchVariance = 0.05f, MaxInstances = 1 });
				SearchString = obj ?? string.Empty;
				OnSearchStringUpdated?.Invoke();
			};
		}));

		ClearTextInputButton = TextInput.AddElement(new UIImageButton(Main.Assets.Request<Texture2D>("Images/UI/SearchCancel")).With(e => {
			e.HAlign = 1f;
			e.VAlign = 0.5f;
			e.Left = StyleDimension.FromPixels(-6f);

			e.AddComponent(new SoundPlaybackUIComponent {
				HoverSound = SoundID.MenuTick,
			});

			e.OnLeftClick += ClearTextInput;
		}));
	}

	private void ClearTextInput(UIMouseEvent evt, UIElement listeningElement)
	{
		if (TextInput.HasContents) {
			TextInput.SetContents(null, forced: true);
			SoundEngine.PlaySound(SoundID.MenuClose);
		} else {
			SoundEngine.PlaySound(SoundID.MenuTick);
		}

		IsFocused = false;
	}

	public override void Update(GameTime gameTime)
	{
		base.Update(gameTime);

		if (InputSystem.GetMouseButtonDown(0)) {
			var mousePosition = UserInterface.ActiveInstance.MousePosition;
			IsFocused = ContainsPoint(mousePosition) && !ClearTextInputButton.ContainsPoint(mousePosition);
		}

		if (Main.keyState.IsKeyDown(Keys.Enter) && !Main.oldKeyState.IsKeyDown(Keys.Enter) && IsFocused) {
			IsFocused = false;
		}
	}
}

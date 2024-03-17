using Microsoft.Xna.Framework;
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

public class EditableUIText : UIElement
{
	public FancyUIPanel Container { get; }
	public UISearchBar TextInput { get; }

	public int MaxTextInputLength { get; set; }
	public string TextContent { get; set; }

	public bool IsFocused {
		get => TextInput.IsWritingText;
		set {
			if (IsFocused != value) {
				TextInput.ToggleTakingText();
			}
		}
	}

	public EditableUIText(string? textContent = null) : base()
	{
		TextContent = textContent ?? string.Empty;

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

		TextInput = Container.AddElement(new UISearchBar(Language.GetText(""), 0.8f).With(e => {
			e.Width = StyleDimension.Fill;
			e.Height = StyleDimension.Fill;
			e.HAlign = 0f;
			e.VAlign = 0.5f;

			e.SetContents(textContent, true);
			e.OnContentsChanged += (string obj) => {
				TextContent = obj.Length <= MaxTextInputLength ? obj : TextContent;
			};
		}));
	}

	public virtual bool TextFilter(string text) => true;

	public void SetText(string text)
	{
		TextContent = text;
		TextInput.SetContents(TextContent, true);
	}

	public override void Update(GameTime gameTime)
	{
		base.Update(gameTime);

		if (InputSystem.GetMouseButtonDown(0)) {
			IsFocused = ContainsPoint(UserInterface.ActiveInstance.MousePosition);
		}

		if (Main.keyState.IsKeyDown(Keys.Enter) && !Main.oldKeyState.IsKeyDown(Keys.Enter)) {
			IsFocused = false;
			SoundEngine.PlaySound(SoundID.MenuTick);
		}
	}
}

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.UI;
using TerrariaOverhaul.Core.Interface;

namespace TerrariaOverhaul.Common.ConfigurationScreen;

public class EditableUIText : UIElement
{
	public FancyUIPanel Container { get; }
	public UISearchBar TextInput { get; }

	public int MaxTextInputLength { get; set; }
	public bool IsFocused { get; set; }
	public string? TextContent { get; set; }

	public EditableUIText(string? textContent = null) : base()
	{
		TextContent = textContent;

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

	public void ToggleFocus()
	{
		TextInput.ToggleTakingText();
		IsFocused = TextInput.IsWritingText;
	}

	public void SetText(string text)
	{
		TextContent = text;
		TextInput.SetContents(TextContent, true);
	}

	public override void LeftClick(UIMouseEvent evt)
	{
		ToggleFocus();
	}

	public override void Update(GameTime gameTime)
	{
		base.Update(gameTime);

		if (Main.keyState.IsKeyDown(Keys.Enter) && !Main.oldKeyState.IsKeyDown(Keys.Enter) && IsFocused) {
			IsFocused = false;
		}
	}
}

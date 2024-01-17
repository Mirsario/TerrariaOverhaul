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
using TerrariaOverhaul.Core.Interface;

namespace TerrariaOverhaul.Common.ConfigurationScreen;
public class EditableUIText : UIElement
{
	FancyUIPanel Container { get; }
	public UISearchBar TextInput { get; }

	public int MaxTextInputLength;

	public bool IsFocused = false;
	public string? textContent;

	public EditableUIText(string? textContent = null) : base()
	{
		this.textContent = textContent;

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
		}));
	}

	public override void LeftClick(UIMouseEvent evt)
	{
		ToggleFocus();
	}

	public void ToggleFocus()
	{
		TextInput.ToggleTakingText();
		IsFocused = TextInput.IsWritingText;
	}

	public override void Update(GameTime gameTime)
	{
		base.Update(gameTime);

		if (Main.keyState.IsKeyDown(Keys.Enter) && !Main.oldKeyState.IsKeyDown(Keys.Enter) && IsFocused) {
			IsFocused = false;
		}
	}
}

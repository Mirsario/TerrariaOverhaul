using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using Terraria.ModLoader.UI.Elements;
using Terraria.UI;
using TerrariaOverhaul.Core.Interface;

namespace TerrariaOverhaul.Common.ConfigurationScreen;

public class SettingsPanel : UIElement
{
	public UIPanel OptionRowsPanel { get; }

	public SettingsPanel() : base()
	{
		// Self

		Width = StyleDimension.Fill;
		Height = StyleDimension.Fill;

		// Main

		var optionRowsContainer = this.AddElement(new UIElement().With(e => {
			e.Width = StyleDimension.Fill;
			e.Height = StyleDimension.FromPercent(0.7f);
		}));

		var optionRowsGridContainer = optionRowsContainer.AddElement(new UIPanel().With(e => {
			e.Width = StyleDimension.FromPixelsAndPercent(-32f, 1f);
			e.Height = StyleDimension.Fill;
			e.BackgroundColor = new Color(54, 68, 128);
			e.BorderColor = new Color(54, 68, 128);
		}));

		var optionRows = optionRowsGridContainer.AddElement(new UIGrid().With(e => {
			e.Width = StyleDimension.Fill;
			e.Height = StyleDimension.Fill;
			e.ListPadding = 6f;
		}));

		var optionRowsScrollbar = optionRowsContainer.AddElement(new UIScrollbar().With(e => {
			e.HAlign = 1f;
			e.VAlign = 0.5f;
			e.Height = StyleDimension.FromPixelsAndPercent(-8f, 1f);

			optionRows.SetScrollbar(e);
		}));

		for (int i = 0; i < 17; i++) {
			optionRows.Add(new UIPanel().With(e => {
				e.Width = StyleDimension.Fill;
				e.Height = StyleDimension.FromPixels(40f);
				e.BackgroundColor = new Color(73, 94, 171);
				e.BorderColor = new Color(42, 54, 99);
			}));
		}

		// Bottom panel

		var descriptionPanel = this.AddElement(new UIPanel().With(e => {
			e.Width = StyleDimension.Fill;
			e.Height = StyleDimension.FromPixelsAndPercent(-12f, 0.3f);
			e.HAlign = 0.5f;
			e.VAlign = 1f;
			e.BackgroundColor = new Color(73, 94, 171);
			e.BorderColor = new Color(42, 54, 99);
		}));

		Console.WriteLine(descriptionPanel.GetOuterDimensions().Height);

		var optionIconContainer = descriptionPanel.AddElement(new UIElement().With(e => {
			e.Height = StyleDimension.FromPixels(112f);
			e.Width = StyleDimension.FromPixels(112f);
			e.VAlign = 0.5f;
		}));

		var unselectedIconBorder = optionIconContainer.AddElement(new UIImage(ModContent.Request<Texture2D>($"{nameof(TerrariaOverhaul)}/Assets/Textures/UI/Config/UnselectedIconBorder")).With(e => {
			e.HAlign = 0.5f;
			e.VAlign = 0.5f;
		}));

		var unselectedIconImage = optionIconContainer.AddElement(new UIImage(Main.Assets.Request<Texture2D>("Images/UI/Bestiary/Icon_Locked")).With(e => {
			e.HAlign = 0.5f;
			e.VAlign = 0.5f;
		}));
	}
}

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
	public UIGrid OptionRowsGrid { get; }
	public UIElement OptionRowsContainer { get; }
	public UIPanel OptionRowsGridContainer { get; }
	public UIScrollbar OptionRowsScrollbar { get; }

	public SettingsPanel() : base()
	{
		// Self

		Width = StyleDimension.Fill;
		Height = StyleDimension.Fill;

		// Main

		OptionRowsContainer = this.AddElement(new UIElement().With(e => {
			e.Width = StyleDimension.Fill;
			e.Height = StyleDimension.FromPercent(0.7f);
		}));

		OptionRowsGridContainer = OptionRowsContainer.AddElement(new UIPanel().With(e => {
			e.Width = StyleDimension.FromPixelsAndPercent(-32f, 1f);
			e.Height = StyleDimension.Fill;
			e.BackgroundColor = new Color(54, 68, 128);
			e.BorderColor = new Color(54, 68, 128);
		}));

		OptionRowsGrid = OptionRowsGridContainer.AddElement(new UIGrid().With(e => {
			e.Width = StyleDimension.Fill;
			e.Height = StyleDimension.Fill;
			e.ListPadding = 6f;
		}));

		OptionRowsScrollbar = OptionRowsContainer.AddElement(new UIScrollbar().With(e => {
			e.HAlign = 1f;
			e.VAlign = 0.5f;
			e.Height = StyleDimension.FromPixelsAndPercent(-8f, 1f);

			OptionRowsGrid.SetScrollbar(e);
		}));

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

	public UIPanel AddOptionRow()
	{
		var panel = new UIPanel().With(e => {
			e.Width = StyleDimension.Fill;
			e.Height = StyleDimension.FromPixels(40f);
			e.BackgroundColor = new Color(73, 94, 171);
			e.BorderColor = new Color(42, 54, 99);
		});

		OptionRowsGrid.Add(panel);

		return panel;
	}
}

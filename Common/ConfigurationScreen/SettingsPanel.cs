using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.UI.Elements;
using Terraria.UI;
using TerrariaOverhaul.Core.Interface;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.ConfigurationScreen;

public class SettingsPanel : UIPanel
{
	public UIPanel OptionRowsPanel { get; }

	public SettingsPanel() : base()
	{
		// Self

		Width = StyleDimension.Fill;
		Height = StyleDimension.Fill;

		SetPadding(12f);

		// Main

		var optionRowsContainer = this.AddElement(new UIElement().With(e => {
			e.Width = StyleDimension.Fill;
			e.Height = StyleDimension.FromPercent(0.7f);
		}));

		var panelGrid = optionRowsContainer.AddElement(new UIGrid().With(e => {
			e.Width = StyleDimension.FromPixelsAndPercent(-20f, 1f);
			e.Height = StyleDimension.Fill;
			e.ListPadding = 6f;

			e.PaddingRight = 12f;
		}));

		var panelGridScrollbar = optionRowsContainer.AddElement(new UIScrollbar().With(e => {
			e.HAlign = 1f;
			e.VAlign = 0.5f;
			e.Height = StyleDimension.FromPixelsAndPercent(-8f, 1f);

			panelGrid.SetScrollbar(e);
		}));

		for (int i = 0; i < 17; i++) {
			panelGrid.Add(new UIPanel().With(e => {
				e.Width = StyleDimension.Fill;
				e.Height = StyleDimension.FromPixels(40f);
				e.BackgroundColor = new Color(73, 94, 171);
			}));
		}

		// Bottom panel

		var descriptionPanel = this.AddElement(new UIPanel().With(e => {
			e.Width = StyleDimension.Fill;
			e.Height = StyleDimension.FromPixelsAndPercent(-12f, 0.3f);
			e.HAlign = 0.5f;
			e.VAlign = 1f;
			e.BackgroundColor = new Color(73, 94, 171);
		}));

		var optionIconContainer = descriptionPanel.AddElement(new UIElement().With(e => {
			e.Height = StyleDimension.Fill;
			e.Width = StyleDimension.FromPixels(129f);
		}));

		var unselectedIconBorder = optionIconContainer.AddElement(new UIImage(ModContent.Request<Texture2D>($"{nameof(TerrariaOverhaul)}/Assets/Textures/UI/Config/UnselectedIconBorder")));

		var unselectedIconImage = unselectedIconBorder.AddElement(new UIImage(Main.Assets.Request<Texture2D>("Images/UI/Bestiary/Icon_Locked")).With(e => {
			e.HAlign = 0.5f;
			e.VAlign = 0.5f;
		}));
	}
}

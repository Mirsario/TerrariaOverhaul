using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
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

	public SettingsPanel(LocalizedText title) : base()
	{
		// Self

		Width = StyleDimension.Fill;
		Height = StyleDimension.Fill;

		SetPadding(0f);

		// Main

		var optionRowsContainer = this.AddElement(new UIElement().With(e => {
			e.Width = StyleDimension.FromPercent(0.7f);
			e.Height = StyleDimension.Fill;
		}));

		var panelGrid = optionRowsContainer.AddElement(new UIGrid().With(e => {
			e.Width = StyleDimension.FromPixelsAndPercent(-20, 1f);
			e.Height = StyleDimension.Fill;
			e.ListPadding = 6f;

			e.SetPadding(12f);
		}));

		var panelGridScrollbar = optionRowsContainer.AddElement(new UIScrollbar().With(e => {
			e.HAlign = 1f;
			e.VAlign = 0.5f;
			e.Height = StyleDimension.FromPixelsAndPercent(-36f, 1f);

			panelGrid.SetScrollbar(e);
		}));

		for (int i = 0; i < 40; i++) {
			panelGrid.Add(new UIPanel() {
				Width = StyleDimension.Fill,
				Height = StyleDimension.FromPixels(40f),
			});
		}

		var descriptionPanel = this.AddElement(new UIPanel().With(e => {
			e.Width = StyleDimension.FromPixelsAndPercent(-27, 0.3f);
			e.Height = StyleDimension.FromPixelsAndPercent(-24, 1f);
			e.Left = StyleDimension.FromPixelsAndPercent(15f, 0.7f);
			e.VAlign = 0.5f;
		}));
	}
}

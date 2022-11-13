using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using Terraria.UI;

namespace TerrariaOverhaul.Core.Configuration.ConfigurationUI;

public class ConfigPanel : UIPanel
{
	public ConfigPanel(string ThumbnailTexture) : base()
	{
		Width = StyleDimension.FromPixels(144f);
		Height = StyleDimension.FromPixels(180f);
		BorderColor = Color.Black;
		BackgroundColor = new Color(73, 94, 171);

		SetPadding(0f);

		UIPanel PanelThumbnailContainer = new() {
			Width = StyleDimension.FromPixels(104f),
			Height = StyleDimension.FromPixels(104f),
			HAlign = 0.5f,
			VAlign = 0.1f,
			BackgroundColor = Color.Red * 0.5f,
			BorderColor = Color.Red * 0.5f
		};

		PanelThumbnailContainer.SetPadding(0f);

		Append(PanelThumbnailContainer);

		UIImage PanelThumbnailBorder = new(/* border image path */) {
			HAlign = 0f,
			VAlign = 0f
		};

		PanelThumbnailContainer.Append(PanelThumbnailBorder);

		/*UIImage PanelThumbnail = new(ModContent.Request<Texture2D>(ThumbnailTexture)) {
			Width = StyleDimension.FromPixels(96f),
			Height = StyleDimension.FromPixels(96f),
			ScaleToFit = true,
			AllowResizingDimensions = true,
			HAlign = 0.5f,
			VAlign = 0.5f
		};

		PanelThumbnailBorder.Append(PanelThumbnail);*/


		/*UIElement PanelTitleContainer = new() {
			Width = StyleDimension.FromPercent(1f),
			Height = StyleDimension.FromPixels(68f),
			VAlign = 1f
		};

		Append(PanelTitleContainer);

		UIText PanelTitle = new("Quandale Dingle Here") {
			IsWrapped = true,
			Width = StyleDimension.FromPercent(1f),
			Height = StyleDimension.FromPercent(1f),
			Top = StyleDimension.FromPixels(14f),
			HAlign = 0f,
			VAlign = 0.5f,
			PaddingLeft = 20f,
			PaddingRight = 20f,
		};

		PanelTitleContainer.Append(PanelTitle);*/
	}
}

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using Terraria.UI;

namespace TerrariaOverhaul.Core.Configuration;

public class ConfigPanel : UIPanel
{
	public ConfigPanel(string thumbnailTexture) : base()
	{
		Width = StyleDimension.FromPixels(144f);
		Height = StyleDimension.FromPixels(180f);
		BorderColor = Color.Black;
		BackgroundColor = new Color(73, 94, 171);

		SetPadding(0f);

		var panelThumbnailContainer = new UIPanel() {
			Width = StyleDimension.FromPixels(104f),
			Height = StyleDimension.FromPixels(104f),
			HAlign = 0.5f,
			VAlign = 0.1f,
			BackgroundColor = Color.Red * 0.5f,
			BorderColor = Color.Red * 0.5f
		};

		panelThumbnailContainer.SetPadding(0f);

		Append(panelThumbnailContainer);

		var panelThumbnailBorder = new UIImage(/* border image path */) {
			HAlign = 0f,
			VAlign = 0f
		};

		panelThumbnailContainer.Append(panelThumbnailBorder);

		/*var panelThumbnail = new UIImage(ModContent.Request<Texture2D>(thumbnailTexture)) {
			Width = StyleDimension.FromPixels(96f),
			Height = StyleDimension.FromPixels(96f),
			ScaleToFit = true,
			AllowResizingDimensions = true,
			HAlign = 0.5f,
			VAlign = 0.5f
		};

		panelThumbnailBorder.Append(panelThumbnail);*/

		/*var panelTitleContainer = new UIElement() {
			Width = StyleDimension.FromPercent(1f),
			Height = StyleDimension.FromPixels(68f),
			VAlign = 1f
		};

		Append(panelTitleContainer);

		var panelTitle = new UIText("Quandale Dingle Here") {
			IsWrapped = true,
			Width = StyleDimension.FromPercent(1f),
			Height = StyleDimension.FromPercent(1f),
			Top = StyleDimension.FromPixels(14f),
			HAlign = 0f,
			VAlign = 0.5f,
			PaddingLeft = 20f,
			PaddingRight = 20f,
		};

		panelTitleContainer.Append(panelTitle);*/
	}
}

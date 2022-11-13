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
		Width = StyleDimension.FromPixels(135f);
		Height = StyleDimension.FromPixels(180f);
		BorderColor = Color.Black;
		BackgroundColor = new Color(73, 94, 171);

		var panelThumbnailContainer = new UIPanel() {
			Width = StyleDimension.FromPixels(112),
			Height = StyleDimension.FromPixels(112f),
			HAlign = 0.5f
		};

		panelThumbnailContainer.SetPadding(0f);

		Append(panelThumbnailContainer);

		/*var panelThumbnailBorder = new UIImage(ModContent.Request<Texture2D>("TerrariaOverhaul/Core/Configuration/_UI/ThumbnailBorder", ReLogic.Content.AssetRequestMode.ImmediateLoad)) {
			ScaleToFit = true,
			Width = StyleDimension.FromPercent(1f),
			Height = StyleDimension.FromPercent(1f)
		};

		panelThumbnailContainer.Append(panelThumbnailBorder);*/

		var panelThumbnail = new UIImage(ModContent.Request<Texture2D>(thumbnailTexture)) {
			Width = StyleDimension.FromPercent(1f),
			Height = StyleDimension.FromPercent(1f),
			ScaleToFit = true
		};

		panelThumbnailContainer.Append(panelThumbnail);

		var panelTitleContainer = new UIElement() {
			Width = StyleDimension.FromPercent(1f),
			Height = StyleDimension.FromPixels(44f),
			VAlign = 1f,
			PaddingTop = 12f,
			PaddingBottom = 12f
		};

		Append(panelTitleContainer);

		var panelTitle = new UIText("Config Title") {
			IsWrapped = true,
			Width = StyleDimension.FromPercent(1f),
			Height = StyleDimension.FromPercent(1f),
			VAlign = 0.5f
		};

		panelTitleContainer.Append(panelTitle);
	}
}

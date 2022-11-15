using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;
using TerrariaOverhaul.Core.Interface;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.ConfigurationScreen;

public class ConfigPanel : UIPanel
{
	private static Asset<Texture2D> defaultBorderTexture = null!;

	public UIElement ThumbnailContainer { get; }
	public UIImage Thumbnail { get; }
	public UIImage ThumbnailBorder { get; }
	public UIElement TitleContainer { get; }
	public UIText Title { get; }

	public ConfigPanel(LocalizedText title, Asset<Texture2D> thumbnailTexture, Asset<Texture2D>? borderTexture = null) : base()
	{
		borderTexture ??= defaultBorderTexture ??= ModContent.Request<Texture2D>($"{GetType().GetFullDirectory()}/ThumbnailBorder");

		thumbnailTexture.Wait?.Invoke();
		borderTexture.Wait?.Invoke();

		// Self

		Width = StyleDimension.FromPixels(135f);
		Height = StyleDimension.FromPixels(190f);
		BorderColor = Color.Black;
		BackgroundColor = new Color(73, 94, 171);

		// Thumbnail

		ThumbnailContainer = this.AddElement(new UIElement().With(e => {
			e.Width = StyleDimension.FromPixels(112);
			e.Height = StyleDimension.FromPixels(112f);
			e.HAlign = 0.5f;
		}));

		Thumbnail = ThumbnailContainer.AddElement(new UIImage(thumbnailTexture).With(e => {
			e.Width = StyleDimension.Fill;
			e.Height = StyleDimension.Fill;
			e.ScaleToFit = true;
		}));

		ThumbnailBorder = Thumbnail.AddElement(new UIImage(borderTexture).With(e => {
			e.ScaleToFit = true;
			e.Width = StyleDimension.Fill;
			e.Height = StyleDimension.Fill;
		}));

		// Title

		TitleContainer = this.AddElement(new UIElement().With(e => {
			e.Width = StyleDimension.Fill;
			e.Height = StyleDimension.FromPixels(54f);
			e.VAlign = 1f;
			e.PaddingTop = 9f;
		}));

		Title = TitleContainer.AddElement(new UIText(title).With(e => {
			e.Width = StyleDimension.Fill;
			e.Height = StyleDimension.Fill;
			e.IsWrapped = true;
		}));
	}
}

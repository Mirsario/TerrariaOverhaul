using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using Terraria.UI;
using TerrariaOverhaul.Core.Interface;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Core.Configuration;

[Autoload(Side = ModSide.Client)]
public class ConfigPanel : UIPanel, ILoadable
{
	private static Asset<Texture2D> defaultBorderTexture = null!;

	public UIElement Container { get; }
	public UIImage Thumbnail { get; }
	public UIPanel ThumbnailContainer { get; }
	public UIImage? ThumbnailBorder { get; }
	public UIText Title { get; }

	public ConfigPanel(string title, Asset<Texture2D> thumbnailTexture, Asset<Texture2D>? borderTexture = null) : base()
	{
		// Self

		Width = StyleDimension.FromPixels(135f);
		Height = StyleDimension.FromPixels(180f);
		BorderColor = Color.Black;
		BackgroundColor = new Color(73, 94, 171);

		// Elements

		ThumbnailContainer = this.AddElement(new UIPanel().With(static e => {
			e.Width = StyleDimension.FromPixels(112);
			e.Height = StyleDimension.FromPixels(112f);
			e.HAlign = 0.5f;

			e.SetPadding(0f);
		}));

		if (borderTexture != null) {
			ThumbnailBorder = ThumbnailContainer.AddElement(new UIImage(borderTexture ?? defaultBorderTexture).With(static e => {
				e.ScaleToFit = true;
				e.Width = StyleDimension.FromPercent(1f);
				e.Height = StyleDimension.FromPercent(1f);
			}));
		}

		Container = this.AddElement(new UIElement().With(static e => {
			e.Width = StyleDimension.FromPercent(1f);
			e.Height = StyleDimension.FromPixels(44f);
			e.VAlign = 1f;
			e.PaddingTop = 12f;
			e.PaddingBottom = 12f;
		}));

		Thumbnail = (ThumbnailBorder ?? Container).AddElement(new UIImage(thumbnailTexture).With(static e => {
			e.Width = StyleDimension.FromPercent(1f);
			e.Height = StyleDimension.FromPercent(1f);
			e.ScaleToFit = true;
		}));

		Title = Container.AddElement(new UIText(title).With(static e => {
			e.IsWrapped = true;
			e.Width = StyleDimension.FromPercent(1f);
			e.Height = StyleDimension.FromPercent(1f);
			e.VAlign = 0.5f;
		}));
	}

	void ILoadable.Load(Mod mod)
	{
		defaultBorderTexture = TextureUtils.GetPlaceholderTexture();
	}

	void ILoadable.Unload() { }
}

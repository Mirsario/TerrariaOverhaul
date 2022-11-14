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

	public UIText Title { get; }
	public UIImage Thumbnail { get; }
	public UIImage ThumbnailBorder { get; }
	public UIPanel ThumbnailContainer { get; }
	public UIElement Container { get; }

	public ConfigPanel(string title, Asset<Texture2D> thumbnailTexture, Asset<Texture2D>? borderTexture = null) : base()
	{
		// Self

		Width = StyleDimension.FromPixels(144f);
		Height = StyleDimension.FromPixels(180f);
		BorderColor = Color.Black;
		BackgroundColor = new Color(73, 94, 171);

		SetPadding(0f);

		// Elements

		ThumbnailContainer = this.AddElement(new UIPanel().With(static e => {
			e.Width = StyleDimension.FromPixels(104f);
			e.Height = StyleDimension.FromPixels(104f);
			e.HAlign = 0.5f;
			e.VAlign = 0.1f;
			e.BackgroundColor = Color.Red * 0.5f;
			e.BorderColor = Color.Red * 0.5f;

			e.SetPadding(0f);
		}));

		ThumbnailBorder = ThumbnailContainer.AddElement(new UIImage(borderTexture ?? defaultBorderTexture).With(static e => {
			e.HAlign = 0f;
			e.VAlign = 0f;
		}));

		Thumbnail = ThumbnailBorder.AddElement(new UIImage(thumbnailTexture).With(static e => {
			e.Width = StyleDimension.FromPixels(96f);
			e.Height = StyleDimension.FromPixels(96f);
			e.ScaleToFit = true;
			e.AllowResizingDimensions = true;
			e.HAlign = 0.5f;
			e.VAlign = 0.5f;
		}));

		Container = this.AddElement(new UIElement().With(static e => {
			e.Width = StyleDimension.FromPercent(1f);
			e.Height = StyleDimension.FromPixels(68f);
			e.VAlign = 1f;
		}));

		Title = Container.AddElement(new UIText(title).With(static e => {
			e.IsWrapped = true;
			e.Width = StyleDimension.FromPercent(1f);
			e.Height = StyleDimension.FromPercent(1f);
			e.Top = StyleDimension.FromPixels(14f);
			e.HAlign = 0f;
			e.VAlign = 0.5f;
			e.PaddingLeft = 20f;
			e.PaddingRight = 20f;
		}));
	}

	void ILoadable.Load(Mod mod)
	{
		defaultBorderTexture = TextureUtils.GetPlaceholderTexture();
	}

	void ILoadable.Unload() { }
}

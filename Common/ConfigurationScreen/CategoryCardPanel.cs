// Copyright (c) 2020-2025 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using ReLogic.Content;
using Terraria;
using Terraria.Localization;
using Terraria.UI;
using TerrariaOverhaul.Core.Interface;

namespace TerrariaOverhaul.Common.ConfigurationScreen;

public class CategoryCardPanel : CardPanel
{
	private readonly UIVideo? video;
	private readonly UIConfigIcon icon;

	public string Category { get; }
	public UIElement ThumbnailNormal { get; private set; }
	public UIElement? ThumbnailHover { get; private set; }

	public CategoryCardPanel(string category, Asset<Texture2D>? backgroundTexture = null, Asset<Texture2D>? borderTexture = null)
		: base(Language.GetText($"Mods.{nameof(TerrariaOverhaul)}.Configuration.{category}.DisplayName"), null, borderTexture)
	{
		Category = category;

		const float BaseResolution = 64f;
		const float MinOutlineSize = 1f;
		var image = ConfigMediaLookup.TryGetMedia(category, "Category", out var imageResult, ConfigMediaKind.Image)
			? (Asset<Texture2D>)imageResult.mediaAsset
			: CommonAssets.UnknownOptionTexture;

		backgroundTexture ??= CommonAssets.GetBackgroundTexture(GetHashCode());

		ThumbnailNormal = icon = new UIConfigIcon(image, backgroundTexture) {
			OutlineSize = Vector2.Max(Vector2.One * MinOutlineSize, image.Size() / (Vector2.One * BaseResolution)),
		};

		if (ConfigMediaLookup.TryGetMedia(category, "Category", out var videoResult, ConfigMediaKind.Video)) {
			ThumbnailHover = video = new UIVideo((Asset<Video>)videoResult.mediaAsset) {
				Width = StyleDimension.Empty,
				Height = StyleDimension.Empty,
				ScaleToFit = true,
				AllowResizingDimensions = false,
			};

			// Non-visible. Rendering is done by UIConfigIcon.
			this.AddElement(video);
		}

		SetThumbnail(ThumbnailNormal);
	}

	public override void OnActivate()
	{
		base.OnActivate();
		Update(Main.gameTimeCache);
	}

	public override void Update(GameTime gameTime)
	{
		base.Update(gameTime);

		if (video?.VideoPlayer is VideoPlayer { State: MediaState.Playing } player) {
			icon.BackgroundTextureOverride = player.GetTexture();
		} else {
			icon.BackgroundTextureOverride = null;
		}
	}

	public override void OnDeactivate()
	{
		base.OnDeactivate();

		icon.BackgroundTextureOverride = null;
	}

	public override void MouseOver(UIMouseEvent evt)
	{
		base.MouseOver(evt);

		if (ThumbnailHover is UIElement thumbnail) {
			SetThumbnail(thumbnail);
		}
	}

	public override void MouseOut(UIMouseEvent evt)
	{
		base.MouseOut(evt);

		SetThumbnail(ThumbnailNormal);
	}
}

using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;
using TerrariaOverhaul.Core.Configuration;
using TerrariaOverhaul.Core.Interface;

namespace TerrariaOverhaul.Common.ConfigurationScreen;

public class ConfigEntryElement : FancyUIPanel
{
	public IConfigEntry ConfigEntry { get; }
	public LocalizedText DisplayName { get; }
	public LocalizedText Description { get; }
	public Asset<Texture2D>? IconTexture { get; }

	public ConfigEntryElement(IConfigEntry configEntry)
	{
		ConfigEntry = configEntry;

		string entryName = configEntry.Name;

		DisplayName = Language.GetText($"Mods.{nameof(TerrariaOverhaul)}.Configuration.{configEntry.Category}.{entryName}.DisplayName");
		Description = Language.GetText($"Mods.{nameof(TerrariaOverhaul)}.Configuration.{configEntry.Category}.{entryName}.Description");

		if (ConfigMediaLookup.TryGetMedia(ConfigEntry, out var mediaTuple, ConfigMediaKind.Image)) {
			IconTexture = (Asset<Texture2D>)mediaTuple.mediaAsset;
		}

		// Self

		SetPadding(0f);

		Width = StyleDimension.Fill;
		Height = StyleDimension.FromPixels(40f);

		Colors.CopyFrom(CommonColors.InnerPanelDarkDynamic);

		// Left side (Text)

		var textContainer = this.AddElement(new UIElement().With(e => {
			e.MaxWidth = e.Width = StyleDimension.Fill;
			e.MaxHeight = e.Height = StyleDimension.Fill;

			e.SetPadding(12f);
		}));

		textContainer.AddElement(new ScrollingUIText(DisplayName).With(e => {
			e.MaxWidth = StyleDimension.FromPercent(0.5f);
		}));

		// Right side (Configuration)

		var configuratorContainer = this.AddElement(new UIElement().With(c => {
			c.MaxWidth = c.Width = StyleDimension.FromPercent(0.5f);
			c.MaxHeight = c.Height = StyleDimension.FromPercent(1.0f);
			c.Left = StyleDimension.FromPercent(0.5f);

			c.SetPadding(0f);
		}));

		if (ConfigElementLookup.TryCreateElement(configEntry, out var element)) {
			element.Left = StyleDimension.FromPercent(0.0f);
			element.Width = StyleDimension.FromPercent(0.5f);
			element.Height = StyleDimension.Fill;
			element.HAlign = 1.0f;

			((IConfigEntryController)element).Value = configEntry.LocalValue;

			configuratorContainer.AddElement(element);
		} else {
			textContainer.AddElement(new UIText("Not supported").With(e => {
				e.HAlign = 1.0f;
				e.VAlign = 0.5f;
				e.TextOriginX = 1.0f;
			}));
		}
	}
}

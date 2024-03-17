using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
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
	public Vector4 CollisionExtents { get; set; }

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

			var controller = (IConfigEntryController)element;

			controller.Value = configEntry.LocalValue;
			controller.OnModified += () => {
				ConfigEntry.LocalValue = Convert.ChangeType(controller.Value, ConfigEntry.ValueType);
				controller.Value = ConfigEntry.LocalValue; // Conversion is to be handled in the getter.

				ConfigIO.SaveConfig();
			};

			configuratorContainer.AddElement(element);
		} else {
			textContainer.AddElement(new UIText("Not supported").With(e => {
				e.HAlign = 1.0f;
				e.VAlign = 0.5f;
				e.TextOriginX = 1.0f;
			}));
		}
	}

	public override bool ContainsPoint(Vector2 point)
	{
		var dimensions = GetDimensions();
		var xyzw = new Vector4(
			dimensions.X - CollisionExtents.X,
			dimensions.Y - CollisionExtents.Y,
			dimensions.X + dimensions.Width + CollisionExtents.Z,
			dimensions.Y + dimensions.Height + CollisionExtents.W
		);

		return point.X > xyzw.X && point.Y > xyzw.Y
			&& point.X < xyzw.Z && point.Y < xyzw.W;
	}
}

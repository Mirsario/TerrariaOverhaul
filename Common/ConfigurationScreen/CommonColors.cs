using Microsoft.Xna.Framework;
using TerrariaOverhaul.Core.Interface;
using static TerrariaOverhaul.Utilities.ColorUtils;

namespace TerrariaOverhaul.Common.ConfigurationScreen;

public struct UIPanelColors
{
	public UIColors Border;
	public UIColors Background;
}

public static class CommonColors
{
	private static class DefaultPalette
	{
		public static readonly Color Darkest = FromHexRgb(0x0b0929);
		public static readonly Color Darker = FromHexRgb(0x181a45);
		public static readonly Color Dark = FromHexRgb(0x272e61);
		public static readonly Color Medium = FromHexRgb(0x364480);
		public static readonly Color Bright = FromHexRgb(0x4b66ab);
		public static readonly Color Brighter = FromHexRgb(0x648acc);
		public static readonly Color Brightest = FromHexRgb(0x81b1eb);
	}

	public static UIPanelColors OuterPanelBright => new() {
		Background = new(DefaultPalette.Bright),
		Border = new(DefaultPalette.Dark),
	};
	public static UIPanelColors OuterPanelMedium => new() {
		Background = new(DefaultPalette.Medium),
		Border = new(DefaultPalette.Darker),
	};
	public static UIPanelColors OuterPanelDark => new() {
		Background = new(DefaultPalette.Dark),
		Border = new(DefaultPalette.Darkest),
	};

	public static UIPanelColors InnerPanelBright => new() {
		Background = new(DefaultPalette.Bright),
		Border = new(DefaultPalette.Brightest, Hover: Color.Gold, Active: Color.White),
	};
	public static UIPanelColors InnerPanelMedium => new() {
		Background = new(DefaultPalette.Medium),
		Border = new(DefaultPalette.Brighter, Hover: Color.Gold, Active: Color.White),
	};
	public static UIPanelColors InnerPanelDark => new() {
		Background = new(DefaultPalette.Dark),
		Border = new(DefaultPalette.Bright, Hover: Color.Gold, Active: Color.White),
	};
}

using Microsoft.Xna.Framework;

namespace TerrariaOverhaul.Utilities;

internal static class ColorUtils
{
	public static Color FromHexRgb(uint hexRgba)
	{
		return new Color(
			(byte)(hexRgba >> 16),
			(byte)(hexRgba >> 8),
			(byte)(hexRgba >> 0),
			255
		);
	}

	public static Color FromHexRgba(uint hexRgba)
	{
		return new Color(
			(byte)(hexRgba >> 24),
			(byte)(hexRgba >> 16),
			(byte)(hexRgba >> 8),
			(byte)(hexRgba >> 0)
		);
	}
}

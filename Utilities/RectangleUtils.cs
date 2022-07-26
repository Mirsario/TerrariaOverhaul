using Microsoft.Xna.Framework;

namespace TerrariaOverhaul.Utilities;

public static class RectangleUtils
{
	public static Rectangle FromPoints(int left, int up, int right, int bottom)
		=> new(left, up, right - left, bottom - up);
}

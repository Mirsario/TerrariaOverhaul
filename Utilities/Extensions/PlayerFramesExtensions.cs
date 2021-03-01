using Microsoft.Xna.Framework;
using TerrariaOverhaul.Utilities.Enums;

namespace TerrariaOverhaul.Utilities.Extensions
{
	public static class PlayerFramesExtensions
	{
		private const int PlayerSheetWidth = 40;
		private const int PlayerSheetHeight = 56;

		public static Rectangle ToRectangle(this PlayerFrames frame)
		{
			return new Rectangle(0, (int)frame * PlayerSheetHeight, PlayerSheetWidth, PlayerSheetHeight);
		}
	}
}

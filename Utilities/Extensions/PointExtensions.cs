using Microsoft.Xna.Framework;
using Terraria;

namespace TerrariaOverhaul.Utilities.Extensions
{
	public static class PointExtensions
	{
		public static bool IsInWorld(this Point point) => point.X >= 0 && point.Y >= 0 && point.X < Main.maxTilesX && point.Y < Main.maxTilesY;
	}
}

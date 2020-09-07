using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;

namespace TerrariaOverhaul.Utilities.Extensions
{
	public static class Vector2Extensions
	{
		public static bool IsInWorld(this Vector2 vec) => vec.X>0 && vec.Y>0 && vec.X<(Main.maxTilesX-1)*16f && vec.Y<(Main.maxTilesY-1)*16f;
	}
}

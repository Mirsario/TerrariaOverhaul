using System;
using Microsoft.Xna.Framework;
using Terraria;

namespace TerrariaOverhaul.Utilities.Terraria;

[Flags]
/// <summary> A mask for liquid IDs. Supports values up to 64. </summary>
internal enum LiquidMask : ulong
{
	None = 0,
	Water = 1 << 0,
	Lava = 1 << 1,
	Honey = 1 << 2,
	Shimmer = 1 << 3,
	All = uint.MaxValue,
}

internal static class LiquidUtils
{
	/// <summary> Performs a check for liquids using two masks. </summary>
	public static bool CheckAreaWithMasks(Rectangle area, LiquidMask skipped, LiquidMask required, byte minLiquid = 1)
	{
		// Exclusive.
		(int checkX1, int checkX2) = (area.X, Math.Min(area.X + area.Width, Main.maxTilesX));
		(int checkY1, int checkY2) = (area.X, Math.Min(area.X + area.Height, Main.maxTilesY));

		for (int checkX = checkX1; checkX < checkX2; checkX++) {
			for (int checkY = checkY1; checkY < checkY2; checkY++) {
				var tile = Main.tile[checkX, checkY];

				if (tile.LiquidAmount < minLiquid) {
					continue;
				}

				var mask = (LiquidMask)(1 << tile.LiquidType);

				if ((mask & skipped) != 0) {
					// This has a liquid that we must avoid.
					return false;
				}

				if (required != 0 && (mask & required) == 0) {
					// This has no liquid that we require.
					return false;
				}
			}
		}

		return true;
	}
}

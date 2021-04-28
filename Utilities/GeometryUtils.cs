using System;
using TerrariaOverhaul.Utilities.DataStructures;

namespace TerrariaOverhaul.Utilities
{
	public static class GeometryUtils
	{
		public delegate void CancellablePositionCallback(Vector2Int position, ref bool stop);

		public static void BresenhamLine(Vector2Int start, Vector2Int end, CancellablePositionCallback action)
		{
			Vector2Int currentPosition = start;

			int width = end.X - start.X;
			int height = end.Y - start.Y;

			Vector2Int stepA = default;
			Vector2Int stepB = default;

			if(width < 0)
				stepA.X = -1;
			else if(width > 0)
				stepA.X = 1;
			if(height < 0)
				stepA.Y = -1;
			else if(height > 0)
				stepA.Y = 1;
			if(width < 0)
				stepB.X = -1;
			else if(width > 0)
				stepB.X = 1;

			int longest = Math.Abs(width);
			int shortest = Math.Abs(height);

			if(longest <= shortest) {
				longest = Math.Abs(height);
				shortest = Math.Abs(width);

				if(height < 0)
					stepB.Y = -1;
				else if(height > 0)
					stepB.Y = 1;

				stepB.X = 0;
			}

			int numerator = longest >> 1;
			bool stop = false;

			for(int i = 0; i <= longest; i++) {
				action(currentPosition, ref stop);

				if(stop) {
					return;
				}

				numerator += shortest;

				if(!(numerator < longest)) {
					numerator -= longest;
					currentPosition += stepA;
				} else {
					currentPosition += stepB;
				}
			}
		}
	}
}

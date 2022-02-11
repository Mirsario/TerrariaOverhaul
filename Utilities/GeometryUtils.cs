using System;

namespace TerrariaOverhaul.Utilities
{
	public static class GeometryUtils
	{
		public delegate void CancellablePositionCallback(Vector2Int position, ref bool stop);
		public delegate void FloodFillCallback(Vector2Int position, out bool occupied, ref bool stop);

		private static bool[,] floodFillVisitCache;

		public static void BresenhamLine(Vector2Int start, Vector2Int end, CancellablePositionCallback action)
		{
			Vector2Int currentPosition = start;

			int width = end.X - start.X;
			int height = end.Y - start.Y;

			Vector2Int stepA = default;
			Vector2Int stepB = default;

			if (width < 0)
				stepA.X = -1;
			else if (width > 0)
				stepA.X = 1;

			if (height < 0)
				stepA.Y = -1;
			else if (height > 0)
				stepA.Y = 1;

			if (width < 0)
				stepB.X = -1;
			else if (width > 0)
				stepB.X = 1;

			int longest = Math.Abs(width);
			int shortest = Math.Abs(height);

			if (longest <= shortest) {
				longest = Math.Abs(height);
				shortest = Math.Abs(width);

				if (height < 0)
					stepB.Y = -1;
				else if (height > 0)
					stepB.Y = 1;

				stepB.X = 0;
			}

			int numerator = longest >> 1;
			bool stop = false;

			for (int i = 0; i <= longest; i++) {
				action(currentPosition, ref stop);

				if (stop) {
					return;
				}

				numerator += shortest;

				if (!(numerator < longest)) {
					numerator -= longest;
					currentPosition += stepA;
				} else {
					currentPosition += stepB;
				}
			}
		}

		public static void FloodFill(Vector2Int start, Vector2Int gridSize, FloodFillCallback callback)
		{
			if (floodFillVisitCache == null || floodFillVisitCache.GetLength(0) < gridSize.X || floodFillVisitCache.GetLength(1) < gridSize.Y) {
				floodFillVisitCache = new bool[gridSize.X, gridSize.Y];
			} else {
				for (int y = 0; y < gridSize.Y; y++) {
					for (int x = 0; x < gridSize.X; x++) {
						floodFillVisitCache[x, y] = false;
					}
				}
			}

			bool stop = false;

			void Recursion(Vector2Int position)
			{
				if (stop || position.X < 0 || position.Y < 0 || position.X >= gridSize.X || position.Y >= gridSize.Y || floodFillVisitCache[position.X, position.Y]) {
					return;
				}

				floodFillVisitCache[position.X, position.Y] = true;

				callback(position, out bool occupied, ref stop);

				if (!occupied && !stop) {
					Recursion(new Vector2Int(position.X - 1, position.Y));
					Recursion(new Vector2Int(position.X + 1, position.Y));
					Recursion(new Vector2Int(position.X, position.Y - 1));
					Recursion(new Vector2Int(position.X, position.Y + 1));
				}
			}

			Recursion(start);
		}
	}
}

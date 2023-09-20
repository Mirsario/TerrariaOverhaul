using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;

namespace TerrariaOverhaul.Utilities;

public static class GeometryUtils
{
	/// <summary>
	/// Enumerator, to be used within a foreach.
	/// </summary>
	public ref struct BresenhamLine
	{
		private readonly int shortest;
		private readonly int longest;
		private readonly Vector2Int stepA;
		private readonly Vector2Int stepB;
		private int i;
		private int numerator;

		public Vector2Int Current { get; private set; }

		public BresenhamLine(Vector2Int start, Vector2Int end)
		{
			int width = end.X - start.X;
			int height = end.Y - start.Y;

			stepA = new Vector2Int(Math.Sign(width), Math.Sign(height));
			stepB = new Vector2Int(Math.Sign(width), 0);
			longest = Math.Abs(width);
			shortest = Math.Abs(height);

			if (longest <= shortest) {
				longest = Math.Abs(height);
				shortest = Math.Abs(width);

				stepB.X = 0;
				stepB.Y = Math.Sign(height);
			}

			i = -1;
			numerator = longest >> 1;
			Current = start;
		}

		public bool MoveNext()
		{
			if (i++ > longest) {
				return false;
			}

			numerator += shortest;

			if (!(numerator < longest)) {
				numerator -= longest;
				Current += stepA;
			} else {
				Current += stepB;
			}

			return true;
		}

		public readonly BresenhamLine GetEnumerator()
			=> this;
	}

	/// <summary>
	/// Enumerator, to be used within a foreach. Every enumeration step should update <see cref="Result.IsPointFree"/>.
	/// </summary>
	public ref struct FloodFill
	{
		public readonly ref struct Result
		{
			public readonly Vector2Int Point;

			//TODO: Replace with a ref field after TML moves to .NET 7.0+.
			private readonly Span<bool> occupied;

			/// <summary> Set this after every MoveNext() call to whether the returned point's neighbours should be enumerated because after it. </summary>
			public readonly bool IsPointFree {
				set => occupied[0] = value;
			}

			public Result(Vector2Int point, ref bool occupied)
			{
				Point = point;
				this.occupied = MemoryMarshal.CreateSpan(ref occupied, 1);
			}
		}

		// Grid
		private int gridLength;
		private Vector2Int gridSize;
		private Vector2Int gridOffset;
		private readonly bool[] visitCache;
		// Stack
		private readonly Vector2Int[] stack;
		private uint stackReads;
		private uint stackWrites;
		// Results
		private Vector2Int localPoint;
		private Vector2Int globalPoint;
		private Result result;
		private bool isPointFree;
		private bool rentedArrays;

		public readonly Result Current => result;

		public FloodFill(Vector2Int startPosition, Rectangle rectangle)
		{
			stackReads = 0;
			stackWrites = 0;
			localPoint = startPosition;
			globalPoint = new Vector2Int(-1, -1);
			result = default;
			isPointFree = true;

			gridSize = new Vector2Int(rectangle.Width, rectangle.Height);
			gridOffset = new Vector2Int(rectangle.X, rectangle.Y);
			gridLength = gridSize.X * gridSize.Y;

			if (gridLength > 0) {
				visitCache = ArrayPool<bool>.Shared.Rent(gridLength);
				stack = ArrayPool<Vector2Int>.Shared.Rent(gridLength);
				rentedArrays = true;

				StackPush(startPosition - gridOffset);
			} else {
				visitCache = Array.Empty<bool>();
				stack = Array.Empty<Vector2Int>();
				rentedArrays = false;
			}
		}

		public void Dispose()
		{
			if (rentedArrays) {
				ArrayPool<bool>.Shared.Return(visitCache, clearArray: true);
				ArrayPool<Vector2Int>.Shared.Return(stack, clearArray: false);
				rentedArrays = false;
			}
		}

		public bool MoveNext()
		{
			if (isPointFree) {
				TryAddingPointToStack(new Vector2Int(localPoint.X - 1, localPoint.Y));
				TryAddingPointToStack(new Vector2Int(localPoint.X + 1, localPoint.Y));
				TryAddingPointToStack(new Vector2Int(localPoint.X, localPoint.Y - 1));
				TryAddingPointToStack(new Vector2Int(localPoint.X, localPoint.Y + 1));
			}

			if (stackReads < stackWrites) {
				localPoint = StackPop();
				globalPoint = localPoint + gridOffset;

				result = new Result(globalPoint, ref isPointFree);
				isPointFree = true;

				return true;
			}

			return false;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void StackPush(Vector2Int point)
			=> stack[stackWrites++] = point;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private Vector2Int StackPop()
			=> stack[stackReads++];

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void TryAddingPointToStack(Vector2Int point)
		{
			// Can this be made branchless?
			if ((point.X < gridSize.X & point.Y < gridSize.Y & point.X >= 0 & point.Y >= 0)) {
				int index = point.Y + (point.X * gridSize.Y);

				if (!visitCache[index]) {
					StackPush(point);
					visitCache[index] = true;
				}
			}
		}

		public readonly FloodFill GetEnumerator()
			=> this;
	}
}

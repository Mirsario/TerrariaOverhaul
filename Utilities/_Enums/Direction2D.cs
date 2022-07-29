using System;

namespace TerrariaOverhaul.Utilities;

[Flags]
public enum Direction2D : byte
{
	None = 0,
	Up = 1,
	Down = 2,
	Left = 4,
	Right = 8,
	TopLeft = Up | Left,
	TopRight = Up | Right,
	BottomLeft = Down | Left,
	BottomRight = Down | Right,
}

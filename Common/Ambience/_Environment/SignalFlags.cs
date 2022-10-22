using System;

namespace TerrariaOverhaul.Common.Ambience;

[Flags]
public enum SignalFlags
{
	Multiply = 0,
	Inverse = 1,
	Max = 2,
	Min = 4,
}

using System;
using System.Runtime.InteropServices;

namespace TerrariaOverhaul.Utilities;

public ref struct ValueOverride<T>
{
	//TODO: Replace with ref field when TML moves to .NET 8.
	private Span<T> valueRef;
	private T oldValue;

	public ValueOverride(ref T valueRef, T value)
	{
		this.valueRef = MemoryMarshal.CreateSpan(ref valueRef, 1);
		oldValue = valueRef;
		valueRef = value;
	}

	public void Dispose()
	{
		valueRef[0] = oldValue;
		this = default;
	}
}

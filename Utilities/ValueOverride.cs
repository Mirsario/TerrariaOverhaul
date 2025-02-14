// Copyright (c) 2020-2025 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using System.Runtime.CompilerServices;

namespace TerrariaOverhaul.Utilities;

public static class ValueOverride
{
	public static ValueOverride<T> Create<T>(ref T reference, T value) => new(ref reference, value);
}

public ref struct ValueOverride<T>
{
	private T oldValue;
	private ref T reference;

	public ValueOverride(ref T reference, T value)
	{
		this.reference = ref reference;
		oldValue = reference;
		reference = value;
	}

	public void Dispose()
	{
		if (!Unsafe.IsNullRef(ref reference)) {
			reference = oldValue;
			oldValue = default!;
			reference = ref Unsafe.NullRef<T>();
		}
	}
}

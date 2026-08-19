// Copyright (c) 2020-2026 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using System.Runtime.CompilerServices;

namespace TerrariaOverhaul.Utilities;

#pragma warning disable CS9084 // Struct member returns 'this' or other instance members by reference

internal struct Counter
{
	public uint Value;

	public readonly bool Active => Value != 0;

	public unsafe Handle Increase()
		=> new(ref Value);

	public void Decrease()
	{
		checked { Value++; }
	}

	public ref struct Handle
	{
		private ref uint counter = ref Unsafe.NullRef<uint>();

		internal Handle(ref uint counter)
		{
			this.counter = ref counter;
			checked { counter++; }
		}

		public void Dispose()
		{
			if (!Unsafe.IsNullRef(ref counter)) {
				checked { counter--; }
				counter = ref Unsafe.NullRef<uint>();
			}
		}
	}
}

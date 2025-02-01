// Copyright (c) 2020-2025 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace TerrariaOverhaul.Utilities;

public struct BitMask<T> : IEnumerable<int> where T : unmanaged, IUnsignedNumber<T>, IBitwiseOperators<T, T, T>, IShiftOperators<T, int, T>
{
	private const MethodImplOptions InlineFlags = MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization;

	public static int BitSize { get; } = Marshal.SizeOf<T>() * 8;

	public T Value;

	public readonly int Size => BitSize;
	public readonly bool IsZero => T.IsZero(Value);

	static BitMask() => Test();

	public BitMask(T value) => Value = value;

	[MethodImpl(InlineFlags)] public readonly bool Get(int index) => !T.IsZero(Value & (T.One << index));
	[MethodImpl(InlineFlags)] public void Set(int index) => Value |= T.One << index;
	[MethodImpl(InlineFlags)] public void Unset(int index) => Value &= ~(T.One << index);

	[MethodImpl(InlineFlags)] public readonly int PopCount() => PopCount(Value);
	[MethodImpl(InlineFlags)] public readonly int LeadingZeroCount() => LeadingZeroCount(Value);
	[MethodImpl(InlineFlags)] public readonly int TrailingZeroCount() => TrailingZeroCount(Value);
	[MethodImpl(InlineFlags)] public readonly int LeadingOneCount() => LeadingZeroCount(~Value);
	[MethodImpl(InlineFlags)] public readonly int TrailingOneCount() => TrailingZeroCount(~Value);

	[MethodImpl(InlineFlags)] public static BitMask<T> operator ~(BitMask<T> a) => new(~a.Value);
	[MethodImpl(InlineFlags)] public static BitMask<T> operator &(BitMask<T> a, BitMask<T> b) => new(a.Value & b.Value);
	[MethodImpl(InlineFlags)] public static BitMask<T> operator |(BitMask<T> a, BitMask<T> b) => new(a.Value & b.Value);

	[MethodImpl(InlineFlags)]
	public static int PopCount(T value)
	{
		if (typeof(T) == typeof(byte)) { return BitOperations.PopCount(Unsafe.As<T, byte>(ref value)); }
		if (typeof(T) == typeof(ushort)) { return BitOperations.PopCount(Unsafe.As<T, ushort>(ref value)); }
		if (typeof(T) == typeof(uint)) { return BitOperations.PopCount(Unsafe.As<T, uint>(ref value)); }
		if (typeof(T) == typeof(ulong)) { return BitOperations.PopCount(Unsafe.As<T, ulong>(ref value)); }
		throw new NotSupportedException();
	}
	[MethodImpl(InlineFlags)]
	public static int LeadingZeroCount(T value)
	{
		if (typeof(T) == typeof(byte)) { return BitOperations.LeadingZeroCount(Unsafe.As<T, byte>(ref value)); }
		if (typeof(T) == typeof(ushort)) { return BitOperations.LeadingZeroCount(Unsafe.As<T, ushort>(ref value)); }
		if (typeof(T) == typeof(uint)) { return BitOperations.LeadingZeroCount(Unsafe.As<T, uint>(ref value)); }
		if (typeof(T) == typeof(ulong)) { return BitOperations.LeadingZeroCount(Unsafe.As<T, ulong>(ref value)); }
		throw new NotSupportedException();
	}
	[MethodImpl(InlineFlags)]
	public static int TrailingZeroCount(T value)
	{
		if (typeof(T) == typeof(byte)) { return BitOperations.TrailingZeroCount(Unsafe.As<T, byte>(ref value)); }
		if (typeof(T) == typeof(ushort)) { return BitOperations.TrailingZeroCount(Unsafe.As<T, ushort>(ref value)); }
		if (typeof(T) == typeof(uint)) { return BitOperations.TrailingZeroCount(Unsafe.As<T, uint>(ref value)); }
		if (typeof(T) == typeof(ulong)) { return BitOperations.TrailingZeroCount(Unsafe.As<T, ulong>(ref value)); }
		throw new NotSupportedException();
	}

	readonly IEnumerator IEnumerable.GetEnumerator() => new Iterator(this);
	readonly IEnumerator<int> IEnumerable<int>.GetEnumerator() => new Iterator(this);

	[Conditional("DEBUG")]
	internal static void Test()
	{
		var mask = new BitMask<ulong>();
		Debug.Assert(mask.Value == 0);
		Debug.Assert(mask.PopCount() == 0);
		Debug.Assert(mask.LeadingZeroCount() == 64);
		Debug.Assert(mask.TrailingZeroCount() == 64);
		Debug.Assert(mask.LeadingOneCount() == 0);
		Debug.Assert(mask.TrailingOneCount() == 0);
		mask.Set(0);
		Debug.Assert(mask.Value == 1);
		Debug.Assert(mask.PopCount() == 1);
		Debug.Assert(mask.LeadingZeroCount() == 63);
		Debug.Assert(mask.TrailingZeroCount() == 0);
		Debug.Assert(mask.LeadingOneCount() == 0);
		Debug.Assert(mask.TrailingOneCount() == 1);
		mask.Set(1);
		Debug.Assert(mask.Value == 3);
		Debug.Assert(mask.PopCount() == 2);
		Debug.Assert(mask.LeadingZeroCount() == 62);
		Debug.Assert(mask.TrailingZeroCount() == 0);
		Debug.Assert(mask.LeadingOneCount() == 0);
		Debug.Assert(mask.TrailingOneCount() == 2);
		mask.Unset(0);
		Debug.Assert(mask.Value == 2);
		Debug.Assert(mask.PopCount() == 1);
		Debug.Assert(mask.LeadingZeroCount() == 62);
		Debug.Assert(mask.TrailingZeroCount() == 1);
		Debug.Assert(mask.LeadingOneCount() == 0);
		Debug.Assert(mask.TrailingOneCount() == 0);
	}

	public struct Iterator(BitMask<T> mask) : IEnumerator<int>
	{
		public BitMask<T> Mask = mask;
		public int Current { get; private set; } = -1;

		readonly object IEnumerator.Current => Current;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool MoveNext()
		{
			if (Mask.IsZero) {
				Current = -1;
				return false;
			}

			Current = Mask.TrailingZeroCount();
			Mask.Unset(Current);
			return true;
		}

		readonly void IDisposable.Dispose() { }
		void IEnumerator.Reset() => throw new NotImplementedException();
	}
}

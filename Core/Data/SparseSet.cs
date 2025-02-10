using System;
using System.Numerics;

namespace TerrariaOverhaul.Core.Data;

/// <summary> Basic sparse set without any entry removal support. </summary>
public struct SparseSet<TData>()
{
	private const int Invalid = -1;

	private int[] sparse = [];
	private TData[] dense = [];

	public int Count { get; private set; }

	public readonly int[] Sparse => sparse;
	public readonly TData[] Dense => dense;

	public SparseSet(int sparseCapacity, int denseCapacity) : this()
	{
		dense = denseCapacity > 0 ? new TData[denseCapacity] : [];
		sparse = sparseCapacity > 0 ? new int[sparseCapacity] : [];
		for (int i = 0; i < sparseCapacity; i++) Sparse[i] = Invalid;
	}

	public readonly bool Has(uint index) => index < sparse.Length && sparse[index] != Invalid;
	public readonly ref TData Get(uint index) => ref dense[sparse[index]];

	public ref TData Put(uint index, in TData value)
	{
		if (index >= sparse.Length) {
			int oldLength = sparse.Length;
			int newLength = (int)BitOperations.RoundUpToPowerOf2((index + 1));
			Array.Resize(ref sparse, newLength);
			for (int i = oldLength; i < newLength; i++) Sparse[i] = Invalid;
		}

		int denseIndex = sparse[index];
		if (denseIndex == Invalid) {
			sparse[index] = denseIndex = Count++;

			if (denseIndex >= dense.Length)
				Array.Resize(ref dense, (int)BitOperations.RoundUpToPowerOf2((uint)(denseIndex + 1)));
		}

		dense[denseIndex] = value;

		return ref dense[denseIndex];
	}
}

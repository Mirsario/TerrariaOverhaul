// Copyright (c) 2020-2025 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using System;

namespace TerrariaOverhaul.Utilities;

public sealed class Surface<T> : IDisposable where T : unmanaged
{
	public T[] Data { get; private set; }
	public int Width { get; private set; }
	public int Height { get; private set; }

	public ref T this[int x, int y] => ref Data[Width * y + x];

	public Surface(int width, int height)
	{
		Data = new T[width * height];
		Width = width;
		Height = height;
	}

	public void Dispose()
	{
		GC.SuppressFinalize(this);
		Data = null!;
		Width = -1;
		Height = -1;
	}
}

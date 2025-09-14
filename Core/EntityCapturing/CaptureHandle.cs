// Copyright (c) 2020-2025 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using System.Collections.Generic;

namespace TerrariaOverhaul.Core.EntityCapturing;

internal ref struct CaptureHandle<T> where T : struct
{
	private Stack<List<T>>? stack;

	public CaptureHandle(Stack<List<T>> stack, List<T> list)
	{
		this.stack = stack;

		stack.Push(list);
	}

	public void Dispose()
	{
		if (stack != null) {
			stack.Pop();

			stack = null;
		}
	}
}

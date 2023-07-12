using System.Collections.Generic;

namespace TerrariaOverhaul.Core.EntityCapturing;

public ref struct CaptureHandle<T>
	where T : struct
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

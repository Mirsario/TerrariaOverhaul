using Terraria;

namespace TerrariaOverhaul.Core.EntityCapturing;

public ref struct CounterHandle
{
	// Can't use ref fields in C# 10.0
	private Ref<uint>? counter;

	public CounterHandle(Ref<uint> counter)
	{
		this.counter = counter;

		checked {
			counter.Value++;
		}
	}

	public void Dispose()
	{
		if (counter != null) {
			checked {
				counter.Value--;
			}

			counter = null;
		}
	}
}

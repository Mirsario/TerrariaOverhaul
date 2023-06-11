namespace TerrariaOverhaul.Utilities;

public struct SingleOrGradient<T> where T : new()
{
	public T Single;
	public Gradient<T>? Gradient;

	public SingleOrGradient()
	{
		this = default;
		Single = new();
	}
}

public struct SingleOrGradient<TSingle, TGradientValue>
	where TSingle : new()
	where TGradientValue : new()
{
	public TSingle Single;
	public Gradient<TGradientValue>? Gradient;

	public SingleOrGradient()
	{
		this = default;
		Single = new();
	}
}

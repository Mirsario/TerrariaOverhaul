// Copyright (c) 2020-2025 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

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

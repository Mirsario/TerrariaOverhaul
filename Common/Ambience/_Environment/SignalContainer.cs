using System;
using TerrariaOverhaul.Core.Tags;

namespace TerrariaOverhaul.Common.Ambience;

public struct SignalContainer
{
	private readonly Tag[] signals;

	public SignalFlags Flags { get; }

	public ReadOnlySpan<Tag> Signals => signals;

	public SignalContainer(params Tag[] signals) : this(0, signals) { }

	public SignalContainer(SignalFlags flags, params Tag[] signals)
	{
		Flags = flags;
		this.signals = signals;
	}
}

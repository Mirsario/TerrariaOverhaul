// Copyright (c) 2020-2026 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using System;

#pragma warning disable IDE0064 // Make readonly fields writable

namespace TerrariaOverhaul.Utilities;

internal ref struct Defer(Action action)
{
	public readonly Action? Action = action;

	public void Dispose()
	{
		if (Action != null) {
			Action.Invoke();
			this = default;
		}
	}
}

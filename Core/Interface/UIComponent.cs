// Copyright (c) 2020-2026 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using System;
using Terraria.UI;

namespace TerrariaOverhaul.Core.Interface;

internal abstract class UIComponent
{
	public UIElement Element { get; private set; } = null!;

	protected abstract void OnAttach();

	protected abstract void OnDetach();

	public void AttachTo(UIElement parent)
	{
		if (Element != null) {
			throw new InvalidOperationException("UI component already attached to an element.");
		}

		Element = parent;

		OnAttach();
	}

	public void Detach()
	{
		if (Element == null) {
			throw new InvalidOperationException("UI component not attached to an element.");
		}

		OnDetach();

		Element = null!;
	}
}

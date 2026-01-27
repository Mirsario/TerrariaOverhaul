// Copyright (c) 2020-2026 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using System;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace TerrariaOverhaul.Core.Interface;

internal sealed class ScrollbarListenerUIComponent : UIComponent
{
	private bool updateNextTick;
	private float? lastViewPosition;

	//TODO: Mark 'required' in .NET 8.
	public UIScrollbar Scrollbar { get; set; } = null!;

	protected override void OnAttach()
	{
		if (Scrollbar == null) {
			throw new ArgumentNullException(nameof(Scrollbar));
		}

		Element.OnUpdate += OnUpdate;
	}

	protected override void OnDetach()
	{
		Element.OnUpdate -= OnUpdate;
	}

	private void OnUpdate(UIElement element)
	{
		bool positionDiff = lastViewPosition != Scrollbar.ViewPosition;

		if (positionDiff || updateNextTick) {
			element.Recalculate();

			updateNextTick = positionDiff; // Update for 2 ticks since the execution order is a mess.
			lastViewPosition = Scrollbar.ViewPosition;
		}
	}
}

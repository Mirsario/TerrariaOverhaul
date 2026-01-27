// Copyright (c) 2020-2026 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using System;
using Terraria.ModLoader.UI.Elements;

namespace TerrariaOverhaul.Common.ConfigurationScreen;

// Centers items when possible.
internal class FancyUIGrid : UIGrid
{
	public override void RecalculateChildren()
	{
		float maxRowWidth = GetInnerDimensions().Width;

		base.RecalculateChildren();

		int index = 0;
		float rowY = 0f;

		do {
			float maxItemWidth = 0f;
			float maxItemHeight = 0f;
			float rowWidth = 0f;
			int rowLength = 0;

			for (int i = index; i < _items.Count; i++) {
				var item = _items[i];
				var itemDimensions = item.GetOuterDimensions();

				if (rowWidth != 0f && rowWidth + itemDimensions.Width > maxRowWidth) {
					break;
				}

				maxItemWidth = Math.Max(maxItemWidth, itemDimensions.Width);
				maxItemHeight = Math.Max(maxItemHeight, itemDimensions.Height);
				rowWidth += itemDimensions.Width + ListPadding;
				rowLength++;
			}

			float rowLeft = Math.Max(0f, maxRowWidth - rowWidth) * 0.5f;

			for (int i = 0; i < rowLength; i++) {
				var item = _items[index + i];
				float left = rowLeft + ((maxItemWidth + ListPadding) * i);

				item.Left.Set(left, 0f);
				item.Top.Set(rowY, 0f);
			}

			index += rowLength;
			rowY += maxItemHeight + ListPadding;
		}
		while (index < _items.Count);

		//this._innerListHeight = top + maxRowHeight;
	}
}

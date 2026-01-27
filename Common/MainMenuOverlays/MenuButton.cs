// Copyright (c) 2020-2026 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using Microsoft.Xna.Framework;
using TerrariaOverhaul.Core.Localization;

namespace TerrariaOverhaul.Common.MainMenuOverlays;

internal abstract class MenuButton : MenuLine
{
	public MenuButton(Text text) : base(text)
	{
		ForcedColor ??= GetColor;
	}

	protected abstract override void OnClicked();

	private static Color GetColor(bool isHovering) => isHovering ? Color.White : Color.LightSlateGray;
}

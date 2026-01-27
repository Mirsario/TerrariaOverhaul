// Copyright (c) 2020-2026 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using Terraria;
using Terraria.Audio;
using Terraria.UI;

namespace TerrariaOverhaul.Core.Interface;

internal sealed class SoundPlaybackUIComponent : UIComponent
{
	private bool wasHovered;
	private bool wasPressed;

	public SoundStyle? HoverSound { get; set; }
	public SoundStyle? ClickSound { get; set; }

	protected override void OnAttach()
		=> Element.OnUpdate += OnUpdate;

	protected override void OnDetach()
		=> Element.OnUpdate -= OnUpdate;

	private void OnUpdate(UIElement element)
	{
		bool isHovered = Element.ContainsPoint(Main.MenuUI.MousePosition);
		bool isPressed = isHovered && Main.mouseLeft;

		if (isHovered && !wasHovered) {
			SoundEngine.PlaySound(HoverSound);
		}

		if (isPressed && !wasPressed) {
			SoundEngine.PlaySound(ClickSound);
		}

		wasHovered = isHovered;
		wasPressed = isPressed;
	}
}

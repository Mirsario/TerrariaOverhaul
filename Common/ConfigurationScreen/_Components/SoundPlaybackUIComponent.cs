using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.UI;
using TerrariaOverhaul.Core.Interface;

namespace TerrariaOverhaul.Common.ConfigurationScreen;

public sealed class SoundPlaybackUIComponent : UIComponent
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

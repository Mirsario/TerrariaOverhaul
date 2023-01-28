using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.UI;

namespace TerrariaOverhaul.Common.ConfigurationScreen;

public class ScrollingUIText : UIText
{
	private bool canScroll;
	private bool scrollingLeft;
	private int cooldownTimer;

	public bool NoScroll { get; set; }
	public UIElement? ScrollStopAssistElement { get; set; }

	public ScrollingUIText(string text, float textScale = 1, bool large = false)
		: base(text, textScale, large) { }

	public ScrollingUIText(LocalizedText text, float textScale = 1, bool large = false)
		: base(text, textScale, large) { }

	public override void Draw(SpriteBatch spriteBatch)
	{
		base.Draw(spriteBatch);

		if (NoScroll) {
			return;
		}

		canScroll = cooldownTimer <= 0;

		if (IsMouseHovering || ScrollStopAssistElement?.IsMouseHovering == true) {
			canScroll = false;
			return;
		}

		if (cooldownTimer > 0) {
			cooldownTimer--;
			return;
		}

		Rectangle cullingArea = GetViewCullingArea();
		Vector2 offset = new(10f);

		if (!scrollingLeft && Parent.ContainsPoint(new Vector2(cullingArea.Right, cullingArea.Bottom) + offset) ||
			scrollingLeft && Parent.ContainsPoint(new Vector2(cullingArea.Left, cullingArea.Bottom) - offset)) {
			scrollingLeft = !scrollingLeft;
			cooldownTimer = 90;

			return;
		}

		if (canScroll) {
			Left.Set(Left.Pixels + 1f * scrollingLeft.ToDirectionInt(), 0f);
		}
	}
}

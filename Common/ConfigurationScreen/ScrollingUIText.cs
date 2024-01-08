using System;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.UI;
using TerrariaOverhaul.Core.Time;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.ConfigurationScreen;

public class ScrollingUIText : UIText
{
	public bool StopOnHover { get; set; }
	public bool NoScroll { get; set; }
	public float ScrollPixelsPerSecond { get; set; } = 128f;
	public float ScrollSpeedSnapStep { get; set; } = 0.5f; // Used to synchronize slightly differing texts.
	public float PauseTimeInSeconds { get; set; } = 1.0f;
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

		if (StopOnHover && (IsMouseHovering || ScrollStopAssistElement?.IsMouseHovering == true)) {
			return;
		}

		float currentTime = (float)TimeSystem.GlobalStopwatch.Elapsed.TotalSeconds;

		var dimensions = GetDimensions();
		var parentDimensions = Parent.GetInnerDimensions();
		float horizontalScrollRange = dimensions.Width - parentDimensions.Width;

		if (horizontalScrollRange <= 0f) {
			return;
		}

		float scrollTime = MathF.Ceiling(horizontalScrollRange / ScrollPixelsPerSecond / ScrollSpeedSnapStep) * ScrollSpeedSnapStep;
		float moduloRange = scrollTime + PauseTimeInSeconds;
		float singleModulo = MathUtils.Modulo(currentTime, moduloRange * 1f);
		float doubleModulo = MathUtils.Modulo(currentTime, moduloRange * 2f);
		bool scrollingLeft = doubleModulo > moduloRange;
		float scrollProgress = MathUtils.Clamp01(singleModulo / scrollTime);

		scrollProgress = scrollingLeft ? 1f - scrollProgress : scrollProgress;

		Left.Set(-(horizontalScrollRange * scrollProgress), 0f);
		HAlign = 0f;
		Recalculate();
	}
}

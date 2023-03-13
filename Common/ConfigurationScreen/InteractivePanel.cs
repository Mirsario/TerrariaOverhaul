using System;
using Microsoft.Xna.Framework;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;
using TerrariaOverhaul.Core.Time;

namespace TerrariaOverhaul.Common.ConfigurationScreen;

public class InteractivePanel : UIPanel
{
	private Color borderColor;
	private Color? borderColorHover;
	private TimeSpan lastOutTime;

	public Color? BorderColorActive { get; set; }
	public SoundStyle? HoverSound { get; set; }

	public new Color BorderColor {
		get => borderColor;
		set {
			borderColor = value;

			ResetBorderColor();
		}
	}
	public Color? BorderColorHover {
		get => borderColorHover;
		set {
			borderColorHover = value;

			ResetBorderColor();
		}
	}

	public override void MouseOver(UIMouseEvent evt)
	{
		base.MouseOver(evt);

		if (BorderColorHover.HasValue) {
			base.BorderColor = BorderColorHover.Value;
		}

		if (HoverSound.HasValue && (TimeSystem.GlobalStopwatch.Elapsed - lastOutTime).TotalSeconds >= 0.05d) {
			SoundEngine.PlaySound(HoverSound.Value);
		}
	}

	public override void MouseOut(UIMouseEvent evt)
	{
		base.MouseOut(evt);

		base.BorderColor = BorderColor;

		lastOutTime = TimeSystem.GlobalStopwatch.Elapsed;
	}

	public override void MouseDown(UIMouseEvent evt)
	{
		if (BorderColorActive.HasValue) {
			base.BorderColor = BorderColorActive.Value;
		}
	}

	public override void MouseUp(UIMouseEvent evt)
	{
		if (BorderColorActive.HasValue) {
			ResetBorderColor();
		}
	}

	private void ResetBorderColor()
	{
		base.BorderColor = IsMouseHovering && BorderColorHover.HasValue ? BorderColorHover.Value : BorderColor;
	}
}

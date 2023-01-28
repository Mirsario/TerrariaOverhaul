using Microsoft.Xna.Framework;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace TerrariaOverhaul.Common.ConfigurationScreen;

public class UIPanelExt : UIPanel
{
	public new Color BorderColor { get; set; }
	public Color? BorderColorHover { get; set; }
	public Color? BorderColorActive { get; set; }

	public override void MouseOver(UIMouseEvent evt)
	{
		base.MouseOver(evt);

		if (BorderColorHover.HasValue) {
			base.BorderColor = BorderColorHover.Value;
		}
	}

	public override void MouseOut(UIMouseEvent evt)
	{
		base.MouseOut(evt);

		base.BorderColor = BorderColor;
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
			base.BorderColor = IsMouseHovering && BorderColorHover.HasValue ? BorderColorHover.Value : BorderColor;
		}
	}
}

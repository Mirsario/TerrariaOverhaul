using Terraria.GameContent.UI.Elements;
using TerrariaOverhaul.Core.Interface;

namespace TerrariaOverhaul.Common.ConfigurationScreen;

public class InteractivePanel : UIPanel
{
	// Components
	public BorderColorsUIComponent BorderColors { get; }
	public SoundPlaybackUIComponent SoundPlayback { get; }

	public InteractivePanel() : base()
	{
		BorderColors = this.AddComponent(new BorderColorsUIComponent());
		SoundPlayback = this.AddComponent(new SoundPlaybackUIComponent());
	}
}

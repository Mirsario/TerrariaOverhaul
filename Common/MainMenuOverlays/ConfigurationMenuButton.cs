using Terraria;
using Terraria.Audio;
using Terraria.ID;
using TerrariaOverhaul.Common.ConfigurationScreen;
using TerrariaOverhaul.Core.Localization;

namespace TerrariaOverhaul.Common.MainMenuOverlays;

public class ConfigurationMenuButton : MenuButton
{
	public ConfigurationMenuButton(Text text) : base(text) { }

	protected override void OnClicked()
	{
		SoundEngine.PlaySound(SoundID.MenuOpen);
		Main.MenuUI.SetState(ConfigurationState.Instance);
		Main.menuMode = 888;
	}
}

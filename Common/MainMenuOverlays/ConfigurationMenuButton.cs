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
		/*
		Process.Start(new ProcessStartInfo(ConfigSystem.ConfigPath) {
			UseShellExecute = true,
			Verb = "open",
		});
		*/

		/*
		string configFilePath = ConfigSystem.ConfigPath;
		string configDirectory = ConfigSystem.ConfigDirectory;
		string configFileName = Path.GetFileName(configFilePath);

		try {
			Terraria.Utils.OpenToURL(configFilePath);
		}
		catch {
			try {
				Terraria.Utils.OpenToURL(configDirectory);
			}
			catch {
				if (Main.menuMode != 888) {
					Terraria.Utils.ShowFancyErrorMessage($"[c/FF7777:Unable to open Overhaul's configuration file for editing.]\r\nPlease manually navigate to [c/9fecf0:{configDirectory}] and modify [c/9fecf0:{configFileName}] with a text editor of your choosing.\r\n\r\nThe lack of a configuration GUI is temporary.", 0);
				}
			}
		}
		*/

		// PRE-CUSTOM GUI LOGIC ^^^

		if (Main.menuMode == MenuID.Title) {
			SoundEngine.PlaySound(SoundID.MenuOpen);
			Main.MenuUI.SetState(new ConfigurationState());
			Main.menuMode = 888;
		} else {
			SoundEngine.PlaySound(SoundID.MenuClose);
			Main.menuMode = MenuID.Title;
		}
	}
}

using System.Diagnostics;
using TerrariaOverhaul.Core.Configuration;
using TerrariaOverhaul.Core.Localization;

namespace TerrariaOverhaul.Common.MainMenuOverlays;

public class ConfigurationMenuButton : MenuButton
{
	public ConfigurationMenuButton(Text text) : base(text) { }

	protected override void OnClicked()
	{
		Process.Start(new ProcessStartInfo(ConfigSystem.ConfigPath) {
			UseShellExecute = true,
			Verb = "open",
		});
	}
}

using System.Diagnostics;
using TerrariaOverhaul.Core.Configuration;

namespace TerrariaOverhaul.Common.MainMenuOverlays
{
	public class ConfigurationMenuButton : MenuButton
	{
		public ConfigurationMenuButton(string text) : base(text) { }

		protected override void OnClicked()
		{
			Process.Start(new ProcessStartInfo(ConfigSystem.ConfigPath) {
				UseShellExecute = true,
				Verb = "open",
			});
		}
	}
}

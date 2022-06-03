using System.Diagnostics;
using Steamworks;
using Terraria.Social;

namespace TerrariaOverhaul.Common.MainMenuOverlays
{
	public class MenuLink : MenuButton
	{
		public string Url { get; }
		public bool PreferSteamBrowser { get; init; } = true; // Set to true for now, since firefox seems to show "Your firefox profile cannot be loaded" errors for some reason.
		
		public MenuLink(string text, string url) : base(text)
		{
			Url = url;
		}

		protected override void OnClicked()
		{
			if (PreferSteamBrowser && SocialAPI.Mode == SocialMode.Steam) {
				SteamFriends.ActivateGameOverlayToWebPage(Url);
				return;
			}

			Process.Start(new ProcessStartInfo(Url) {
				UseShellExecute = true
			});
		}
	}
}

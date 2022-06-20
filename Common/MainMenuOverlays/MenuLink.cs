using System.Diagnostics;
using Steamworks;
using Terraria.Social;

namespace TerrariaOverhaul.Common.MainMenuOverlays
{
	public class MenuLink : MenuButton
	{
		public string Url { get; }
		public bool PreferSteamBrowser { get; init; }
		
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

			Terraria.Utils.OpenToURL(Url);
		}
	}
}

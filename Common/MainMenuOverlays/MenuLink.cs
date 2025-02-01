// Copyright (c) 2020-2025 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using Steamworks;
using Terraria.Social;
using TerrariaOverhaul.Core.Localization;

namespace TerrariaOverhaul.Common.MainMenuOverlays;

public class MenuLink : MenuButton
{
	public string Url { get; }
	public bool PreferSteamBrowser { get; init; }
	
	public MenuLink(Text text, string url) : base(text)
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

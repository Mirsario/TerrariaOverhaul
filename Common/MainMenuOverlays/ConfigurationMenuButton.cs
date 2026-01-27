// Copyright (c) 2020-2026 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using System.Reflection;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.ConfigurationScreen;
using TerrariaOverhaul.Core.Localization;

namespace TerrariaOverhaul.Common.MainMenuOverlays;

internal class ConfigurationMenuButton : MenuButton
{
	public ConfigurationMenuButton(Text text) : base(text) { }

	protected override void OnClicked()
	{
		if (typeof(ModLoader).GetField("isLoading", BindingFlags.NonPublic | BindingFlags.Static)?.GetValue(null) is true) {
			return;
		}

		SoundEngine.PlaySound(SoundID.MenuOpen);
		Main.MenuUI.SetState(ConfigurationState.Instance);
		Main.menuMode = 888;
	}
}

// Copyright (c) 2020-2026 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using Terraria.ModLoader;
using TerrariaOverhaul.Utilities.Terraria;

namespace TerrariaOverhaul.Common.Movement;

internal sealed class PlayerWings : ModPlayer
{
	public GameTimer WingsCooldown { get; set; }

	public override void PreUpdate()
	{
		// No Wings Time
		if (WingsCooldown.Active) {
			Player.wingsLogic = 0;
		}
	}
}

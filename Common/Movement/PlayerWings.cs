// Copyright (c) 2020-2025 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using Terraria.ModLoader;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.Movement;

public sealed class PlayerWings : ModPlayer
{
	public Timer WingsCooldown { get; set; }

	public override void PreUpdate()
	{
		// No Wings Time
		if (WingsCooldown.Active) {
			Player.wingsLogic = 0;
		}
	}
}

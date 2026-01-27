// Copyright (c) 2020-2026 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using Terraria;
using Terraria.ModLoader;

namespace TerrariaOverhaul.Common.Hooks.Items;

internal sealed class HoldItemWhileDeadHookImplementation : ModPlayer
{
	public override void UpdateDead()
	{
		var heldItem = Player.HeldItem;

		if (heldItem?.IsAir == false) {
			IHoldItemWhileDead.Invoke(heldItem, Player);
		}
	}
}

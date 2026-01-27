// Copyright (c) 2020-2026 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using Terraria;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.Camera;
using TerrariaOverhaul.Core.ItemComponents;
using TerrariaOverhaul.Core.Time;
using TerrariaOverhaul.Utilities.Terraria;

namespace TerrariaOverhaul.Common.Charging;

[Autoload(Side = ModSide.Client)]
internal sealed class ItemPowerAttackScreenShake : ItemComponent
{
	public ScreenShake ScreenShake;

	private GameTimer lastCharge;

	public override void HoldItem(Item item, Player player)
	{
		if (!Enabled || !player.IsLocal()) {
			return;
		}

		if (!item.TryGetGlobalItem(out ItemPowerAttacks powerAttacks)) {
			return;
		}

		var charge = powerAttacks.Charge;

		if (charge.Active && charge != lastCharge) {
			var screenShake = ScreenShake with {
				LengthInSeconds = charge.Length * TimeSystem.LogicDeltaTime,
			};

			ScreenShakeSystem.New(screenShake, null);

			lastCharge = charge;
		}
	}
}

using Terraria;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.Camera;
using TerrariaOverhaul.Core.ItemComponents;
using TerrariaOverhaul.Core.Time;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.Charging;

[Autoload(Side = ModSide.Client)]
public sealed class ItemPowerAttackScreenShake : ItemComponent
{
	public ScreenShake ScreenShake;

	private Timer lastCharge;

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

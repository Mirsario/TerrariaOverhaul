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

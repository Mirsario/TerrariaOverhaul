using Terraria;
using Terraria.ModLoader;
using TerrariaOverhaul.Utilities;
using static TerrariaOverhaul.Common.Interaction.WaterAndRainInteractions;

namespace TerrariaOverhaul.Common.Interaction;

public sealed class PlayerWaterInteraction : ModPlayer
{
	public override void PostUpdate()
	{
		if (!Player.IsLocal()) {
			return;
		}

		int debuffIndex = Player.FindBuffIndex(DebuffId);

		if (debuffIndex > 0 && Player.buffTime[debuffIndex] > DebuffTimeThreshold) {
			return;
		}

		if (LiquidCheck(Player) || RainCheck(Player)) {
			Player.AddBuff(DebuffId, DebuffTime, quiet: false);
		}
	}

	// Fix for dripping particles being spawned while underwater
	public override void PostUpdateBuffs()
	{
		Player.dripping &= !Player.wet;
	}
}

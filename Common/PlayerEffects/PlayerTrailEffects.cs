using System;
using Terraria;
using Terraria.ModLoader;

namespace TerrariaOverhaul.Common.PlayerEffects;

[Autoload(Side = ModSide.Client)]
public sealed class PlayerTrailEffects : ModPlayer
{
	private int forceTrailEffectTime;

	public override void Load()
	{
		On_Player.SetArmorEffectVisuals += (orig, player, drawPlayer) => {
			orig(player, drawPlayer);

			var modPlayer = drawPlayer.GetModPlayer<PlayerTrailEffects>();

			if (modPlayer.forceTrailEffectTime > 0) {
				player.armorEffectDrawShadow = true;

				modPlayer.forceTrailEffectTime--;
			}
		};
	}

	public void ForceTrailEffect(int forTicks)
	{
		forceTrailEffectTime = Math.Max(forceTrailEffectTime, forTicks);
	}
}

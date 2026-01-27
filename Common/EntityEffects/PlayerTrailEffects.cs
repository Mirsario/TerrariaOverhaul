// Copyright (c) 2020-2026 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using System;
using Terraria;
using Terraria.ModLoader;

namespace TerrariaOverhaul.Common.EntityEffects;

[Autoload(Side = ModSide.Client)]
internal sealed class PlayerTrailEffects : ModPlayer
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

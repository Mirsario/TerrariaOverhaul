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

        public override void PostUpdate()
        {
                if (forceTrailEffectTime > 0) {
                        Player.armorEffectDrawShadow = true;
                        forceTrailEffectTime--;
                }
        }

        public void ForceTrailEffect(int forTicks)
        {
                forceTrailEffectTime = Math.Max(forceTrailEffectTime, forTicks);
        }
}

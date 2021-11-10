using System;
using Terraria;
using Terraria.ModLoader;

namespace TerrariaOverhaul.Common.ModEntities.Players
{
	//TODO: Make this not be instanced on servers.
	public sealed class PlayerEffects : ModPlayer
	{
		private int forceTrailEffectTime;

		public override void Load()
		{
			if (Main.dedServ) {
				return;
			}

			On.Terraria.Player.SetArmorEffectVisuals += (orig, player, drawPlayer) => {
				orig(player, drawPlayer);

				var modPlayer = drawPlayer.GetModPlayer<PlayerEffects>();

				if (modPlayer.forceTrailEffectTime > 0) {
					player.armorEffectDrawShadow = true;

					modPlayer.forceTrailEffectTime--;
				}
			};
		}

		public void ForceTrailEffect(int forTicks) => forceTrailEffectTime = Math.Max(forceTrailEffectTime, forTicks);
	}
}

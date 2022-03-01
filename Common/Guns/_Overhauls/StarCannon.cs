﻿using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace TerrariaOverhaul.Common.Guns
{
	public class StarCannon : Minigun
	{
		public override bool ShouldApplyItemOverhaul(Item item)
		{
			if (item.useAmmo != AmmoID.FallenStar) {
				return false;
			}

			if (item.UseSound != null && item.UseSound != SoundID.Item9) {
				return false;
			}

			return true;
		}

		public override void SetDefaults(Item item)
		{
			base.SetDefaults(item);

			item.UseSound = new ModSoundStyle($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Items/Guns/StarCannon/StarCannonFire", 0, volume: 0.2f, pitchVariance: 0.2f);
		}
	}
}

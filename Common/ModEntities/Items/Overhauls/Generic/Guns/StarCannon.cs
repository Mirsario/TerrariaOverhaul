using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.ModEntities.Players;

namespace TerrariaOverhaul.Common.ModEntities.Items.Overhauls.Generic.Guns
{
	public class StarCannon : Minigun
	{
		public override bool ShouldApplyItemOverhaul(Item item)
		{
			if(item.useAmmo != AmmoID.FallenStar) {
				return false;
			}

			if(item.UseSound != SoundID.Item9) {
				return false;
			}

			return true;
		}
		public override void SetDefaults(Item item)
		{
			item.UseSound = new ModSoundStyle(nameof(TerrariaOverhaul), "Assets/Sounds/Items/Guns/StarCannon/StarCannonFire", 0, volume: 0.2f, pitchVariance: 0.2f);
		}
		public override bool UseItem(Item item, Player player)
		{
			var lookDirection = (player.GetModPlayer<PlayerDirectioning>().mouseWorld - player.Center).SafeNormalize(Vector2.UnitY);

			if(lookDirection.Y > 0f) {
				float bonusYSpeed = -(lookDirection.Y * lookDirection.Y) * 5.5f;

				player.velocity.Y += bonusYSpeed;
			}

			return base.UseItem(item, player);
		}
	}
}

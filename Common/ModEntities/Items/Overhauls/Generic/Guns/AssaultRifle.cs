using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.Systems.Camera.ScreenShakes;

namespace TerrariaOverhaul.Common.ModEntities.Items.Overhauls.Generic.Guns
{
	public class AssaultRifle : AdvancedItem
	{
		public override float OnUseVisualRecoil => 10f;
		public override ScreenShake OnUseScreenShake => new ScreenShake(4f, 0.2f);

		public override bool ShouldApplyItemOverhaul(Item item) => item.useAmmo == AmmoID.Bullet && item.UseSound == SoundID.Item31;

		public override void SetDefaults(Item item)
		{
			item.UseSound = new ModSoundStyle(nameof(TerrariaOverhaul), "Assets/Sounds/Items/Guns/AssaultRifle/AssaultRifleFire", 3, volume: 0.07f, pitchVariance: 0.2f);
		}
		public override bool UseItem(Item item, Player player)
		{
			if(item.UseSound != null && player.itemAnimation < player.itemAnimationMax - 1) {
				Terraria.Audio.SoundEngine.PlaySound(item.UseSound);
			}

			return base.UseItem(item, player);
		}
	}
}

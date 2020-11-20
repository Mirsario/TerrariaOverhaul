using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.Systems.Camera.ScreenShakes;

namespace TerrariaOverhaul.Common.ModEntities.Items.Overhauls.Generic.Guns
{
	public class Shotgun : AdvancedItem
	{
		public ModSoundStyle pumpSound;

		private uint pumpTime;

		public override float OnUseVisualRecoil => 25f;
		public override ScreenShake OnUseScreenShake => new ScreenShake(10f, 0.25f);

		public override bool ShouldApplyItemOverhaul(Item item) => item.useAmmo == AmmoID.Bullet && (item.UseSound == SoundID.Item36 || item.UseSound == SoundID.Item38);

		public override void SetDefaults(Item item)
		{
			item.UseSound = new ModSoundStyle(nameof(TerrariaOverhaul), "Assets/Sounds/Items/Guns/Shotgun/ShotgunFire", 4, volume: 0.2f, pitchVariance: 0.2f);
			pumpSound = new ModSoundStyle(nameof(TerrariaOverhaul), "Assets/Sounds/Items/Guns/Shotgun/ShotgunPump", 0, volume: 0.25f, pitchVariance: 0.1f);
		}
		public override bool UseItem(Item item, Player player)
		{
			if(!base.UseItem(item, player)) {
				return false;
			}

			pumpTime = (uint)(Main.GameUpdateCount + player.itemAnimationMax / 2);

			return true;
		}
		public override void HoldItem(Item item, Player player)
		{
			if(!Main.dedServ && pumpSound != null && pumpTime != 0 && Main.GameUpdateCount == pumpTime) {
				SoundEngine.PlaySound(pumpSound, player.Center);
			}
		}
	}
}

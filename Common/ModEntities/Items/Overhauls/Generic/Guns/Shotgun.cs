using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.Systems.Camera.ScreenShakes;
using TerrariaOverhaul.Content.Gores;

namespace TerrariaOverhaul.Common.ModEntities.Items.Overhauls.Generic.Guns
{
	public class Shotgun : Gun
	{
		public ModSoundStyle pumpSound;
		public int shellCount = 1;

		private uint pumpTime;

		public override float OnUseVisualRecoil => 25f;
		public override ScreenShake OnUseScreenShake => new(10f, 0.25f);

		public override bool ShouldApplyItemOverhaul(Item item) => item.useAmmo == AmmoID.Bullet && (item.UseSound == SoundID.Item36 || item.UseSound == SoundID.Item38);

		public override void SetDefaults(Item item)
		{
			item.UseSound = new ModSoundStyle(nameof(TerrariaOverhaul), "Assets/Sounds/Items/Guns/Shotgun/ShotgunFire", 4, volume: 0.2f, pitchVariance: 0.2f);
			pumpSound = new ModSoundStyle(nameof(TerrariaOverhaul), "Assets/Sounds/Items/Guns/Shotgun/ShotgunPump", 0, volume: 0.25f, pitchVariance: 0.1f);

			switch(item.type) {
				default:
					shellCount = 1;
					break;
				case ItemID.Boomstick:
					shellCount = 2;
					break;
				case ItemID.QuadBarrelShotgun:
					shellCount = 4;
					break;
			}
		}
		
		public override bool? UseItem(Item item, Player player)
		{
			bool? baseResult = base.UseItem(item, player);

			if(baseResult == false) {
				return false;
			}

			pumpTime = (uint)(Main.GameUpdateCount + player.itemAnimationMax / 2);

			return baseResult;
		}
		
		public override void HoldItem(Item item, Player player)
		{
			base.HoldItem(item, player);

			if(!Main.dedServ && pumpSound != null && pumpTime != 0 && Main.GameUpdateCount == pumpTime) {
				SoundEngine.PlaySound(pumpSound, player.Center);
				SpawnCasings<ShellCasing>(player, shellCount);
			}
		}
	}
}

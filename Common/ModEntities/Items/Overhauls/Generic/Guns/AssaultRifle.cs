using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.Systems.Camera.ScreenShakes;
using TerrariaOverhaul.Content.Gores;

namespace TerrariaOverhaul.Common.ModEntities.Items.Overhauls.Generic.Guns
{
	public class AssaultRifle : Gun
	{
		public override float OnUseVisualRecoil => 10f;
		public override ScreenShake OnUseScreenShake => new(4f, 0.2f);

		public override bool ShouldApplyItemOverhaul(Item item)
		{
			//Rifles always use bullets.
			if(item.useAmmo != AmmoID.Bullet) {
				return false;
			}

			//Require ClockworkAssaultRifle's sound. TODO: This should also somehow accept other sounds, and also avoid conflicting with handgun/minigun overhauls. Width/height ratios can help with the former.
			if(item.UseSound != SoundID.Item31) {
				return false;
			}

			return true;
		}
		
		public override void SetDefaults(Item item)
		{
			item.UseSound = new ModSoundStyle(nameof(TerrariaOverhaul), "Assets/Sounds/Items/Guns/AssaultRifle/AssaultRifleFire", 3, volume: 0.125f, pitchVariance: 0.2f);
			PlaySoundOnEveryUse = true;
		}
		
		public override bool? UseItem(Item item, Player player)
		{
			if(!Main.dedServ) {
				SpawnCasings<BulletCasing>(player);
			}

			return base.UseItem(item, player);
		}
	}
}

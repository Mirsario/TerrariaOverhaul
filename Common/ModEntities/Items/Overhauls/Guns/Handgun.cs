using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.Systems.Camera.ScreenShakes;
using TerrariaOverhaul.Content.Gores;

namespace TerrariaOverhaul.Common.ModEntities.Items.Overhauls.Guns
{
	public class Handgun : Gun
	{
		public override float OnUseVisualRecoil => 13f;
		public override ScreenShake OnUseScreenShake => new(4f, 0.2f);

		public override bool ShouldApplyItemOverhaul(Item item)
		{
			if(item.useAmmo != AmmoID.Bullet) {
				return false;
			}

			if((item.UseSound != SoundID.Item41 || item.useTime < 6) && (item.UseSound != SoundID.Item11 || item.useTime < 10)) {
				return false;
			}

			return true;
		}
		
		public override void SetDefaults(Item item)
		{
			item.UseSound = new ModSoundStyle($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Items/Guns/Handgun/HandgunFire", 0, volume: 0.15f, pitchVariance: 0.2f);
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

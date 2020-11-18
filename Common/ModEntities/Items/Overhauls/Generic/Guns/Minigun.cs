using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.Systems.Camera.ScreenShakes;

namespace TerrariaOverhaul.Common.ModEntities.Items.Overhauls.Generic.Guns
{
	public class Minigun : AdvancedItem
	{
		public override float OnUseVisualRecoil => 5f;
		public override ScreenShake OnUseScreenShake => new ScreenShake(5f, 0.25f);

		public override bool ShouldApplyItemOverhaul(Item item) => item.UseSound == SoundID.Item11 && item.useAmmo == AmmoID.Bullet;

		public override void SetDefaults(Item item)
		{
			item.UseSound = new ModSoundStyle(nameof(TerrariaOverhaul), "Assets/Sounds/Items/Guns/Minigun/MinigunFire", 0, volume: 0.15f, pitchVariance: 0.2f);
		}
	}
}

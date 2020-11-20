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

		public override bool ShouldApplyItemOverhaul(Item item)
		{
			if(item.UseSound != SoundID.Item11 && item.UseSound != SoundID.Item40 && item.UseSound != SoundID.Item41) {
				return false;
			}

			//Exclude slow firing guns.
			if(item.useTime >= 10) {
				return false;
			}

			return true;
		}

		public override void SetDefaults(Item item)
		{
			item.UseSound = new ModSoundStyle(nameof(TerrariaOverhaul), "Assets/Sounds/Items/Guns/Minigun/MinigunFire", 0, volume: 0.15f, pitchVariance: 0.2f);
		}
	}
}

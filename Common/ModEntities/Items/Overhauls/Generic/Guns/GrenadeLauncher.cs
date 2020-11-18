using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.Systems.Camera.ScreenShakes;

namespace TerrariaOverhaul.Common.ModEntities.Items.Overhauls.Generic.Guns
{
	public class GrenadeLauncher : AdvancedItem
	{
		public override float OnUseVisualRecoil => 18f;
		public override ScreenShake OnUseScreenShake => new ScreenShake(8f, 0.4f);

		public override bool ShouldApplyItemOverhaul(Item item)
		{
			if(item.useAmmo != AmmoID.Rocket) {
				return false;
			}

			if(!ContentSamples.ProjectilesByType.TryGetValue(item.shoot, out var proj)) {
				return false;
			}

			//Prefer things that shoot projectiles with gravity, i.e. grenades.
			if(proj.aiStyle != ProjAIStyleID.GroundProjectile && proj.aiStyle != ProjAIStyleID.Explosive) {
				return false;
			}

			return true;
		}

		public override void SetDefaults(Item item)
		{
			item.UseSound = new ModSoundStyle(nameof(TerrariaOverhaul), "Assets/Sounds/Items/Guns/GrenadeLauncher/GrenadeLauncherFire", 0, volume: 0.15f);
		}
	}
}

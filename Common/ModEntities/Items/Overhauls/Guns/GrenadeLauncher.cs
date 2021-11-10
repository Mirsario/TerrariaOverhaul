using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.Systems.Camera.ScreenShakes;
using TerrariaOverhaul.Common.Tags;

namespace TerrariaOverhaul.Common.ModEntities.Items.Overhauls.Guns
{
	public class GrenadeLauncher : Gun
	{
		public override float OnUseVisualRecoil => 18f;
		public override ScreenShake OnUseScreenShake => new(8f, 0.4f);

		public override bool ShouldApplyItemOverhaul(Item item)
		{
			if (item.useAmmo != AmmoID.Rocket) {
				return false;
			}

			if (!ContentSamples.ProjectilesByType.TryGetValue(item.shoot, out var proj)) {
				return false;
			}

			if (proj.aiStyle != ProjAIStyleID.Explosive || OverhaulProjectileTags.Rocket.Has(proj.type)) {
				return false;
			}

			return true;
		}

		public override void SetDefaults(Item item)
		{
			item.UseSound = new ModSoundStyle($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Items/Guns/GrenadeLauncher/GrenadeLauncherFire", 0, volume: 0.15f);
		}
	}
}

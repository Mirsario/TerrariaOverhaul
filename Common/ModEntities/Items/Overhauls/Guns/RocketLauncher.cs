using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.ModEntities.Items.Components;
using TerrariaOverhaul.Common.Systems.Camera.ScreenShakes;
using TerrariaOverhaul.Common.Tags;

namespace TerrariaOverhaul.Common.ModEntities.Items.Overhauls.Guns
{
	public class RocketLauncher : Gun
	{
		public override bool ShouldApplyItemOverhaul(Item item)
		{
			if (item.useAmmo != AmmoID.Rocket) {
				return false;
			}

			if (!ContentSamples.ProjectilesByType.TryGetValue(item.shoot, out var proj)) {
				return false;
			}

			if (proj.aiStyle != ProjAIStyleID.Explosive || OverhaulProjectileTags.Grenade.Has(proj.type)) {
				return false;
			}

			return true;
		}

		public override void SetDefaults(Item item)
		{
			item.UseSound = new ModSoundStyle($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Items/Guns/RocketLauncher/RocketLauncherFire", 0, volume: 0.35f);

			if (!Main.dedServ) {
				item.AddComponent<ItemUseVisualRecoil>(c => {
					c.Power = 6f;
				});

				item.AddComponent<ItemUseScreenShake>(c => {
					c.ScreenShake = new ScreenShake(8f, 0.4f);
				});
			}
		}
	}
}

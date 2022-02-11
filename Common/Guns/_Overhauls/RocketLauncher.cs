using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.Camera;
using TerrariaOverhaul.Common.Crosshairs;
using TerrariaOverhaul.Common.ModEntities.Items.Components;
using TerrariaOverhaul.Common.Recoil;
using TerrariaOverhaul.Common.Tags;
using TerrariaOverhaul.Core.ItemComponents;
using TerrariaOverhaul.Core.ItemOverhauls;

namespace TerrariaOverhaul.Common.Guns
{
	public class RocketLauncher : ItemOverhaul
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
			base.SetDefaults(item);

			item.UseSound = new ModSoundStyle($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Items/Guns/RocketLauncher/RocketLauncherFire", 0, volume: 0.35f);

			if (!Main.dedServ) {
				item.EnableComponent<ItemAimRecoil>();
				item.EnableComponent<ItemMuzzleflashes>();
				item.EnableComponent<ItemCrosshairController>();

				item.EnableComponent<ItemUseVisualRecoil>(c => {
					c.Power = 6f;
				});

				item.EnableComponent<ItemUseScreenShake>(c => {
					c.ScreenShake = new ScreenShake(8f, 0.4f);
				});
			}
		}
	}
}

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.Camera;
using TerrariaOverhaul.Common.ModEntities.Items.Components;
using TerrariaOverhaul.Common.Tags;
using TerrariaOverhaul.Core.ItemComponents;

namespace TerrariaOverhaul.Common.Guns
{
	public class GrenadeLauncher : Gun
	{
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
			base.SetDefaults(item);

			item.UseSound = new ModSoundStyle($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Items/Guns/GrenadeLauncher/GrenadeLauncherFire", 0, volume: 0.15f);

			if (!Main.dedServ) {
				item.EnableComponent<ItemUseVisualRecoil>(c => {
					c.Power = 18f;
				});

				item.EnableComponent<ItemUseScreenShake>(c => {
					c.ScreenShake = new ScreenShake(8f, 0.4f);
				});
			}
		}
	}
}

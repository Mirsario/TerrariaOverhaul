using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.ModEntities.Items.Components;
using TerrariaOverhaul.Common.Camera;
using TerrariaOverhaul.Content.Gores;
using TerrariaOverhaul.Core.ItemComponents;

namespace TerrariaOverhaul.Common.Guns
{
	public class Handgun : Gun
	{
		public override bool ShouldApplyItemOverhaul(Item item)
		{
			if (item.useAmmo != AmmoID.Bullet) {
				return false;
			}

			if ((item.UseSound != SoundID.Item41 || item.useTime < 6) && (item.UseSound != SoundID.Item11 || item.useTime < 10)) {
				return false;
			}

			return true;
		}

		public override void SetDefaults(Item item)
		{
			base.SetDefaults(item);

			item.UseSound = new ModSoundStyle($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Items/Guns/Handgun/HandgunFire", 0, volume: 0.15f, pitchVariance: 0.2f);

			if (!Main.dedServ) {
				item.EnableComponent<ItemUseVisualRecoil>(c => {
					c.Power = 13f;
				});

				item.EnableComponent<ItemUseScreenShake>(c => {
					c.ScreenShake = new ScreenShake(4f, 0.2f);
				});
			}
		}

		public override bool? UseItem(Item item, Player player)
		{
			if (!Main.dedServ) {
				SpawnCasings<BulletCasing>(player);
			}

			return base.UseItem(item, player);
		}
	}
}

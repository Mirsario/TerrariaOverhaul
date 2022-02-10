using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.ModEntities.Items.Components;
using TerrariaOverhaul.Common.Camera.ScreenShakes;
using TerrariaOverhaul.Content.Gores;

namespace TerrariaOverhaul.Common.ModEntities.Items.Overhauls.Guns
{
	public class AssaultRifle : Gun
	{
		public override bool ShouldApplyItemOverhaul(Item item)
		{
			// Rifles always use bullets.
			if (item.useAmmo != AmmoID.Bullet) {
				return false;
			}

			// Require ClockworkAssaultRifle's sound. TODO: This should also somehow accept other sounds, and also avoid conflicting with handgun/minigun overhauls. Width/height ratios can help with the former.
			if (item.UseSound != SoundID.Item31) {
				return false;
			}

			return true;
		}

		public override void SetDefaults(Item item)
		{
			base.SetDefaults(item);

			item.UseSound = new ModSoundStyle($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Items/Guns/AssaultRifle/AssaultRifleFire", 3, volume: 0.125f, pitchVariance: 0.2f);

			if (!Main.dedServ) {
				item.EnableComponent<ItemPlaySoundOnEveryUse>();

				item.EnableComponent<ItemUseVisualRecoil>(c => {
					c.Power = 10f;
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

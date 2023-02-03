using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.Camera;
using TerrariaOverhaul.Common.Crosshairs;
using TerrariaOverhaul.Common.Items;
using TerrariaOverhaul.Common.Recoil;
using TerrariaOverhaul.Content.Gores;
using TerrariaOverhaul.Core.ItemComponents;
using TerrariaOverhaul.Core.ItemOverhauls;

namespace TerrariaOverhaul.Common.Guns;

public class AssaultRifle : ItemOverhaul
{
	public static readonly SoundStyle AssaultRifleFireSound = new SoundStyle($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Items/Guns/AssaultRifle/AssaultRifleFire", 3) {
		Volume = 0.125f,
		PitchVariance = 0.2f,
	};

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

		item.UseSound = AssaultRifleFireSound;

		if (!Main.dedServ) {
			item.EnableComponent<ItemAimRecoil>();
			item.EnableComponent<ItemMuzzleflashes>();
			item.EnableComponent<ItemPlaySoundOnEveryUse>();

			item.EnableComponent<ItemUseVisualRecoil>(c => {
				c.Power = 10f;
			});

			item.EnableComponent<ItemUseScreenShake>(c => {
				c.ScreenShake = new ScreenShake(0.45f, 0.2f);
			});

			item.EnableComponent<ItemBulletCasings>(c => {
				c.CasingGoreType = ModContent.GoreType<BulletCasing>();
			});
		}
	}
}

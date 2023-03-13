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

public class Handgun : ItemOverhaul
{
	public static readonly SoundStyle HandgunFireSound = new($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Items/Guns/Handgun/HandgunFire") {
		Volume = 0.15f,
		PitchVariance = 0.2f,
	};

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

		item.UseSound = HandgunFireSound;

		if (!Main.dedServ) {
			item.EnableComponent<ItemAimRecoil>();
			item.EnableComponent<ItemBulletCasings>(c => {
				c.CasingGoreType = ModContent.GoreType<BulletCasing>();
			});
		}
	}
}

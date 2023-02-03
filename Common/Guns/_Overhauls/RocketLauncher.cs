using Terraria;
using Terraria.Audio;
using Terraria.ID;
using TerrariaOverhaul.Common.Camera;
using TerrariaOverhaul.Common.Crosshairs;
using TerrariaOverhaul.Common.Items;
using TerrariaOverhaul.Common.Recoil;
using TerrariaOverhaul.Common.Tags;
using TerrariaOverhaul.Core.ItemComponents;
using TerrariaOverhaul.Core.ItemOverhauls;

namespace TerrariaOverhaul.Common.Guns;

public class RocketLauncher : ItemOverhaul
{
	public static readonly SoundStyle RocketLauncherFireSound = new($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Items/Guns/RocketLauncher/RocketLauncherFire") {
		Volume = 0.35f,
		PitchVariance = 0.2f,
	};

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

		item.UseSound = RocketLauncherFireSound;

		if (!Main.dedServ) {
			item.EnableComponent<ItemAimRecoil>();
			item.EnableComponent<ItemMuzzleflashes>();

			item.EnableComponent<ItemUseScreenShake>(c => {
				c.ScreenShake = new ScreenShake(0.6f, 0.25f);
			});
		}
	}
}

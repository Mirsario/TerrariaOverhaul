// Copyright (c) 2020-2025 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using Terraria;
using Terraria.Audio;
using Terraria.ID;
using TerrariaOverhaul.Common.Recoil;
using TerrariaOverhaul.Core.ItemComponents;
using TerrariaOverhaul.Core.ItemOverhauls;
using TerrariaOverhaul.Core.Tags;
using TerrariaOverhaul.Utilities.Terraria;

namespace TerrariaOverhaul.Common.Guns;

public class RocketLauncher : ItemOverhaul
{
	public static readonly SoundStyle RocketLauncherFireSound = new($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Items/Guns/RocketLauncher/RocketLauncherFire") {
		Volume = 0.35f,
		PitchVariance = 0.2f,
	};

	private static readonly ContentSet Grenade = nameof(Grenade);

	public override bool ShouldApplyItemOverhaul(Item item)
	{
		if (item.useAmmo != AmmoID.Rocket) {
			return false;
		}

		if (!ContentSampleUtils.TryGetProjectile(item.shoot, out var proj)) {
			return false;
		}

		if (proj.aiStyle != ProjAIStyleID.Explosive || Grenade.Has(proj)) {
			return false;
		}

		return true;
	}

	public override void SetDefaults(Item item)
	{
		base.SetDefaults(item);

		if (Guns.EnableGunSoundReplacements) {
			item.UseSound = RocketLauncherFireSound;
		}

		if (!Main.dedServ) {
			item.EnableComponent<ItemAimRecoil>();
		}
	}
}

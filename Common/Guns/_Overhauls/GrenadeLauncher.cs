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
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.Guns;

public class GrenadeLauncher : ItemOverhaul
{
	public static readonly SoundStyle GrenadeLauncherFireSound = new($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Items/Guns/GrenadeLauncher/GrenadeLauncherFire") {
		Volume = 0.15f,
		PitchVariance = 0.2f,
	};

	private static readonly ContentSet Rocket = nameof(Rocket);

	public override bool ShouldApplyItemOverhaul(Item item)
	{
		if (item.useAmmo != AmmoID.Rocket) {
			return false;
		}

		if (!ContentSampleUtils.TryGetProjectile(item.shoot, out var projectile)) {
			return false;
		}

		if (projectile.aiStyle != ProjAIStyleID.Explosive || Rocket.Has(projectile)) {
			return false;
		}

		return true;
	}

	public override void SetDefaults(Item item)
	{
		base.SetDefaults(item);

		if (Guns.EnableGunSoundReplacements) {
			item.UseSound = GrenadeLauncherFireSound;
		}

		if (!Main.dedServ) {
			item.EnableComponent<ItemAimRecoil>();
		}
	}
}

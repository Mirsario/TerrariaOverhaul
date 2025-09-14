// Copyright (c) 2020-2025 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;
using TerrariaOverhaul.Core.Configuration;
using TerrariaOverhaul.Core.Tags;

namespace TerrariaOverhaul.Common.ProjectileEffects;

[Autoload(Side = ModSide.Client)]
internal class ProjectileRicochetSound : GlobalProjectile
{
	public static readonly ConfigEntry<bool> EnableBulletImpactAudio = new(ConfigSide.ClientOnly, true, "Guns");

	public static readonly SoundStyle RicochetSound = new($"{nameof(TerrariaOverhaul)}/Assets/Sounds/HitEffects/Ricochet", 2) {
		Volume = 0.1f,
	};
	private static readonly ContentSet Bullet = nameof(Bullet);

	public override bool AppliesToEntity(Projectile projectile, bool lateInstantiation)
		=> Bullet.Has(projectile);

	public override bool OnTileCollide(Projectile projectile, Vector2 oldVelocity)
	{
		if (EnableBulletImpactAudio) {
			SoundEngine.PlaySound(RicochetSound, projectile.Center);
		}

		return true;
	}
}

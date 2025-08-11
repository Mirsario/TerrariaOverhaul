// Copyright (c) 2020-2025 Mirsario, Rartrin, & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Utilities.Terraria;

namespace TerrariaOverhaul.Common.Fires;

public sealed class ProjectileFlames : GlobalProjectile
{
	public override void OnKill(Projectile projectile, int timeLeft)
	{
		if (
			projectile.type
			is not ProjectileID.Flames
			and not ProjectileID.FireArrow
			and not ProjectileID.CursedArrow
			and not ProjectileID.ShadowFlameArrow
			and not ProjectileID.FrostburnArrow
			and not ProjectileID.IchorArrow
			and not ProjectileID.Flames
			and not ProjectileID.ExplosiveBullet
		) {
			return;
		}

		var centerCoords = projectile.Center.ToTileCoordinates();
		int s = projectile.type == ProjectileID.Flames ? 1 : 1;

		for (int y = -s; y <= s; y++) {
			for (int x = -s; x <= s; x++) {
				if (Math.Abs(x) == s && Math.Abs(x) == Math.Abs(y)) continue;
				var coords = centerCoords + new Point(x, y);
				var kind = projectile.type switch {
					ProjectileID.CursedArrow => FlameKind.Cursed,
					ProjectileID.FrostburnArrow => FlameKind.Frost,
					ProjectileID.IchorArrow => FlameKind.Ichor,
					ProjectileID.ShadowFlameArrow => FlameKind.Demon,
					_ => FlameKind.Default,
				};
				
				FireSystem.SetFlame(coords, new TileFlame {
					Strength = 1,
					Extinguish = 0,
					FlameKind = kind,
					SpreadsLeft = 5,
					LengthInSeconds = 10,
				});
			}
		}
	}
}

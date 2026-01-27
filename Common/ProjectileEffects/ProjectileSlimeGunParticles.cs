// Copyright (c) 2020-2026 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.BloodAndGore;

namespace TerrariaOverhaul.Common.ProjectileEffects;

[Autoload(Side = ModSide.Client)]
internal sealed class ProjectileSlimeGunParticles : GlobalProjectile
{
	public override void AI(Projectile projectile)
	{
		if (projectile.type == ProjectileID.SlimeGun) {
			Span<ParticleSystem.ParticleData> particleSpan = stackalloc ParticleSystem.ParticleData[1] {
				new() {
					Position = projectile.Center,
					Velocity = projectile.velocity * 60f + Main.rand.NextVector2Circular(20f, 20f),
					Color = new Color(0, 80, 255, 100),
					IsViolent = false,
				}
			};

			ParticleSystem.SpawnParticles(particleSpan);
		}
	}
}

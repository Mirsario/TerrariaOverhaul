// Copyright (c) 2020-2025 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using Microsoft.Xna.Framework;
using Terraria;

namespace TerrariaOverhaul.Utilities.Terraria;

internal static class ProjectileUtils
{
	public static float RealSpeed(this Projectile proj)
		=> proj.velocity.Length() * (1 + proj.extraUpdates);

	public static Vector2 RealVelocity(this Projectile proj)
		=> proj.velocity * (1 + proj.extraUpdates);

	public static Player? GetOwner(this Projectile proj)
	{
		if (Main.player.IndexInRange(proj.owner)) {
			if (Main.player[proj.owner] is { active: true } player)
				return player;
		}

		return null;
	}
}

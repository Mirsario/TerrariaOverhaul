using Microsoft.Xna.Framework;
using Terraria;

namespace TerrariaOverhaul.Utilities
{
	public static class ProjectileExtensions
	{
		public static float RealSpeed(this Projectile proj) => proj.velocity.Length() * (1 + proj.extraUpdates);
		public static Vector2 RealVelocity(this Projectile proj) => proj.velocity * (1 + proj.extraUpdates);

		public static Player GetOwner(this Projectile proj)
		{
			if (Main.player.IndexInRange(proj.owner)) {
				var player = Main.player[proj.owner];

				if (player != null && player.active) {
					return player;
				}
			}

			return null;
		}
	}
}

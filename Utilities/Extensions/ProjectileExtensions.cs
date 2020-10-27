using Microsoft.Xna.Framework;
using Terraria;

namespace TerrariaOverhaul.Utilities.Extensions
{
	public static class ProjectileExtensions
	{
		public static float RealSpeed(this Projectile proj) => proj.velocity.Length() * (1 + proj.extraUpdates);
		public static Vector2 RealVelocity(this Projectile proj) => proj.velocity * (1 + proj.extraUpdates);
	}
}

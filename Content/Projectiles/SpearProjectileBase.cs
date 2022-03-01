using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace TerrariaOverhaul.Content.Projectiles
{
	public abstract class SpearProjectileBase : ModProjectile
	{
		protected abstract float HoldoutRangeMin { get; }
		protected abstract float HoldoutRangeMax { get; }

		protected virtual int UseDuration(Player player) => player.itemAnimationMax;

		public override void SetDefaults()
		{
			Projectile.penetrate = -1;
			Projectile.aiStyle = ProjAIStyleID.Spear;
			Projectile.friendly = true;
			Projectile.tileCollide = false;
			Projectile.ownerHitCheck = true;
			Projectile.timeLeft = int.MaxValue;
		}

		public override bool PreAI()
		{
			Player player = Main.player[Projectile.owner];

			player.heldProj = Projectile.whoAmI;
			player.itemTime = player.itemAnimation;

			int realDuration = (int)(UseDuration(player) * player.meleeSpeed);

			if (Projectile.timeLeft == int.MaxValue) {
				Projectile.timeLeft = realDuration;
			}

			Projectile.velocity = Vector2.Normalize(Projectile.velocity);

			float halfDuration = realDuration * 0.5f;
			float progress = Projectile.timeLeft > halfDuration
				? (realDuration - Projectile.timeLeft) / halfDuration
				: Projectile.timeLeft / halfDuration;

			Projectile.Center = player.MountedCenter + Vector2.SmoothStep(Projectile.velocity * HoldoutRangeMin, Projectile.velocity * HoldoutRangeMax, progress);

			return false;
		}
	}
}

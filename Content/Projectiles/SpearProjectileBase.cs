using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace TerrariaOverhaul.Content.Projectiles
{
	public abstract class SpearProjectileBase : ModProjectile
    {
        protected abstract int UseDuration { get; }
		protected abstract float HoldoutRangeMin { get; }
		protected abstract float HoldoutRangeMax { get; }

		public sealed override void SetDefaults()
		{
			Projectile.penetrate = -1;
			Projectile.aiStyle = ProjAIStyleID.Spear;
			Projectile.friendly = true;
			Projectile.timeLeft = UseDuration;
			Projectile.tileCollide = false;
		}

		public override bool PreAI()
		{
			Player player = Main.player[Projectile.owner];

			player.heldProj = Projectile.whoAmI;
			player.itemTime = player.itemAnimation;

			int realDuration = (int)(UseDuration * player.meleeSpeed);

			if(Projectile.timeLeft == UseDuration) {
				Projectile.timeLeft = realDuration;
			}

			Projectile.velocity = Vector2.Normalize(Projectile.velocity);
			//Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

			float progress = Projectile.timeLeft > realDuration / 2f
				? (realDuration - Projectile.timeLeft) / (realDuration / 2f)
				: Projectile.timeLeft / (realDuration / 2f);

			Projectile.Center = player.MountedCenter + Vector2.SmoothStep(Projectile.velocity * HoldoutRangeMin, Projectile.velocity * HoldoutRangeMax, progress);

			return false;
		}
	}
}

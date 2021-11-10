using Terraria;
using TerrariaOverhaul.Common.Systems.Decals;

namespace TerrariaOverhaul.Content.Projectiles
{
	public class MopProjectile : SpearProjectileBase
	{
		protected override float HoldoutRangeMin => 40f;
		protected override float HoldoutRangeMax => 80f;

		public override void SetDefaults()
		{
			base.SetDefaults();

			Projectile.width = 16;
			Projectile.height = 16;
		}

		public override void PostAI()
		{
			base.PostAI();

			if (!Main.dedServ) {
				DecalSystem.ClearDecals(Projectile.getRect());
			}
		}
	}
}

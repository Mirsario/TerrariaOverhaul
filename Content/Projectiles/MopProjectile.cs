using Terraria;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.Systems.Decals;
using TerrariaOverhaul.Content.Items.Tools;

namespace TerrariaOverhaul.Content.Projectiles
{
	public class MopProjectile : SpearProjectileBase
	{
		protected override int UseDuration => 13;
		protected override float HoldoutRangeMin => 40f;
		protected override float HoldoutRangeMax => 80f;

		public override void PostAI()
		{
			base.PostAI();

			if(!Main.dedServ) {
				DecalSystem.ClearDecals(Projectile.getRect());
			}
		}
	}
}

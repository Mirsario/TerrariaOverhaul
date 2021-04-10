using Terraria;
using Terraria.DataStructures;

namespace TerrariaOverhaul.Common.ModEntities.Projectiles
{
	public sealed class ProjectileSourceItemInfo : GlobalProjectileBase
	{
		public bool Available { get; private set; }
		public int UseTime { get; private set; }
		public int UseAnimation { get; private set; }

		public override bool InstancePerEntity => true;

		public override void Load()
		{
			base.Load();

			//TODO: Use the OnSpawn hook when it's added.
			On.Terraria.Projectile.NewProjectile_IProjectileSource_float_float_float_float_int_int_float_int_float_float += (orig, source, x, y, speedX, speedY, type, damage, knockback, Owner, ai0, ai1) => {
				int id = orig(source, x, y, speedX, speedY, type, damage, knockback, Owner, ai0, ai1);

				if(id != Main.maxProjectiles && source is ProjectileSource_Item itemSource) {
					var projectile = Main.projectile[id];
					var info = projectile.GetGlobalProjectile<ProjectileSourceItemInfo>();

					info.UseTime = itemSource.Item.useTime;
					info.UseAnimation = itemSource.Item.useAnimation;
					info.Available = true;
				}

				return id;
			};
		}
	}
}

using Terraria;
using Terraria.DataStructures;

namespace TerrariaOverhaul.Common.ModEntities.Projectiles
{
	public sealed class ProjectileSourceItemInfo : GlobalProjectileBase
	{
		public bool Available { get; private set; }
		public int UseTime { get; private set; }
		public int UseAnimation { get; private set; }
		public int ManaUse { get; private set; }

		public override bool InstancePerEntity => true;

		public override void Load()
		{
			base.Load();

			//TODO: Use the OnSpawn hook when it's added.
			On.Terraria.Projectile.NewProjectile_IProjectileSource_float_float_float_float_int_int_float_int_float_float += (orig, source, x, y, speedX, speedY, type, damage, knockback, Owner, ai0, ai1) => {
				int id = orig(source, x, y, speedX, speedY, type, damage, knockback, Owner, ai0, ai1);

				if (id != Main.maxProjectiles) {
					var projectile = Main.projectile[id];

					if (source is ProjectileSource_Item itemSource) {
						var info = projectile.GetGlobalProjectile<ProjectileSourceItemInfo>();

						info.UseTime = itemSource.Item.useTime;
						info.UseAnimation = itemSource.Item.useAnimation;
						info.ManaUse = itemSource.Item.mana;
						info.Available = true;
					} else if (source is ProjectileSource_ProjectileParent parentSource) {
						var parentInfo = parentSource.ParentProjectile.GetGlobalProjectile<ProjectileSourceItemInfo>();

						if (parentInfo.Available) {
							var info = projectile.GetGlobalProjectile<ProjectileSourceItemInfo>();

							info.UseTime = parentInfo.UseTime;
							info.UseAnimation = parentInfo.UseAnimation;
							info.ManaUse = parentInfo.ManaUse;
							info.Available = true;
						}
					}
				}

				return id;
			};
		}
	}
}

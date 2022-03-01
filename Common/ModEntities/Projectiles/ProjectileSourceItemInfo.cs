using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace TerrariaOverhaul.Common.ModEntities.Projectiles
{
	public sealed class ProjectileSourceItemInfo : GlobalProjectile
	{
		public bool Available { get; private set; }
		public int UseTime { get; private set; }
		public int UseAnimation { get; private set; }
		public int ManaUse { get; private set; }

		public override bool InstancePerEntity => true;

		public override void OnSpawn(Projectile projectile, IEntitySource source)
		{
			if (source is EntitySource_ItemUse itemSource) {
				UseTime = itemSource.Item.useTime;
				UseAnimation = itemSource.Item.useAnimation;
				ManaUse = itemSource.Item.mana;
				Available = true;
			} else if (source is EntitySource_ProjectileParent parentSource) {
				var parentInfo = parentSource.ParentProjectile.GetGlobalProjectile<ProjectileSourceItemInfo>();

				if (parentInfo.Available) {
					UseTime = parentInfo.UseTime;
					UseAnimation = parentInfo.UseAnimation;
					ManaUse = parentInfo.ManaUse;
					Available = true;
				}
			}
		}
	}
}

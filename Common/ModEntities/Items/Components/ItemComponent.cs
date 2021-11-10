using Terraria;
using Terraria.ModLoader;

namespace TerrariaOverhaul.Common.ModEntities.Items.Components
{
	public abstract class ItemComponent : GlobalItem
	{
		public bool Enabled { get; set; }

		public override bool InstancePerEntity => true;

		public override GlobalItem Clone(Item item, Item itemClone)
			=> base.Clone(item, itemClone);
	}
}

using Terraria;
using Terraria.ModLoader;
using TerrariaOverhaul.Core.Systems.Debugging;

namespace TerrariaOverhaul.Common.ModEntities.Items.Components
{
	public abstract class ItemComponent : GlobalItem
	{
		public bool Enabled { get; set; }

		public override bool InstancePerEntity => true;

		public override GlobalItem Clone(Item item, Item itemClone)
		{
			return base.Clone(item, itemClone);
		}
	}
}

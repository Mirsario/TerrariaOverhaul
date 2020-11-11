using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace TerrariaOverhaul.Common.ModEntities.Items.Overhauls
{
	public abstract class ItemOverhaul : GlobalItem
	{
		private static List<ItemOverhaul> itemOverhauls;

		protected Item item;

		public override bool InstancePerEntity => true;

		public abstract bool ShouldApplyItemOverhaul(Item item);

		public override bool InstanceForEntity(Item item) => ChooseItemOverhaul(item) == this;
		//
		public override void Load()
		{
			if(itemOverhauls == null) {
				itemOverhauls = new List<ItemOverhaul>();
			}

			itemOverhauls.Add(this);
		}
		public override void Unload()
		{
			if(itemOverhauls != null) {
				itemOverhauls.Clear();

				itemOverhauls = null;
			}
		}
		public override GlobalItem Clone(Item item, Item itemClone)
		{
			var clone = base.Clone(item, itemClone);

			((ItemOverhaul)clone).item = itemClone;

			return clone;
		}

		public static ItemOverhaul ChooseItemOverhaul(Item item)
		{
			//May need some sort of priority system in the future. And cache?
			for(int i = 0; i < itemOverhauls.Count; i++) {
				var itemOverhaul = itemOverhauls[i];

				if(itemOverhaul.ShouldApplyItemOverhaul(item)) {
					return itemOverhaul;
				}
			}

			return null;
		}
	}
}

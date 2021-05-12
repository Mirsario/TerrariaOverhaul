using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Terraria;
using Terraria.ModLoader;

namespace TerrariaOverhaul.Common.ModEntities.Items.Overhauls
{
	public abstract class ItemOverhaul : GlobalItem
	{
		private static List<ItemOverhaul> itemOverhauls = new();
		private static Dictionary<int, int> itemIdMapping = new();

		protected Item item;

		public override bool InstancePerEntity => true;

		public abstract bool ShouldApplyItemOverhaul(Item item);

		public override bool AppliesToEntity(Item item, bool lateInstantiation) => lateInstantiation && ChooseItemOverhaul(item) == this;
		//
		public override void Load()
		{
			int id = itemOverhauls.Count;
			var attachments = GetType().GetCustomAttribute<ItemAttachmentAttribute>();

			if(attachments != null) {
				foreach(int itemId in attachments.ItemIds) {
					itemIdMapping[itemId] = id;
				}
			}

			itemOverhauls.Add(this);
		}
		public override void Unload()
		{
			itemOverhauls.Clear();
			itemIdMapping.Clear();
		}
		public override GlobalItem Clone(Item item, Item itemClone)
		{
			var clone = base.Clone(item, itemClone);

			((ItemOverhaul)clone).item = itemClone;

			return clone;
		}

		public static ItemOverhaul ChooseItemOverhaul(Item item)
		{
			if(itemIdMapping.TryGetValue(item.type, out int overhaulId)) {
				return itemOverhauls[overhaulId];
			}

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

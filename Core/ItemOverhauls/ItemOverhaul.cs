using System.Collections.Generic;
using System.Reflection;
using Terraria;
using Terraria.ModLoader;

namespace TerrariaOverhaul.Core.ItemOverhauls
{
	// Maybe this should be a separate type that doesn't contain anything but ShouldApplyItemOverhaul & SetDefaults.
	// -- Mirsario.

	/// <summary>
	/// A <see cref="GlobalItem"/> category, out of which only one instance can be present on an item at a time.
	/// </summary>
	public abstract class ItemOverhaul : GlobalItem
	{
		private static readonly List<ItemOverhaul> ItemOverhauls = new();
		private static readonly Dictionary<int, int> ItemIdMapping = new();

		protected Item item;

		public override bool InstancePerEntity => true;

		public abstract bool ShouldApplyItemOverhaul(Item item);

		public override bool AppliesToEntity(Item item, bool lateInstantiation)
			=> lateInstantiation && ChooseItemOverhaul(item) == this;

		public override void Load()
		{
			int id = ItemOverhauls.Count;
			var attachments = GetType().GetCustomAttribute<ItemAttachmentAttribute>();

			if (attachments != null) {
				foreach (int itemId in attachments.ItemIds) {
					ItemIdMapping[itemId] = id;
				}
			}

			ItemOverhauls.Add(this);
		}

		public override void Unload()
		{
			ItemOverhauls.Clear();
			ItemIdMapping.Clear();
		}

		public override GlobalItem Clone(Item item, Item itemClone)
		{
			var clone = base.Clone(item, itemClone);

			((ItemOverhaul)clone).item = itemClone;

			return clone;
		}

		public static ItemOverhaul ChooseItemOverhaul(Item item)
		{
			if (ItemIdMapping.TryGetValue(item.type, out int overhaulId)) {
				return ItemOverhauls[overhaulId];
			}

			// May need some sort of priority system in the future. And cache?
			for (int i = 0; i < ItemOverhauls.Count; i++) {
				var itemOverhaul = ItemOverhauls[i];

				if (itemOverhaul.ShouldApplyItemOverhaul(item)) {
					return itemOverhaul;
				}
			}

			return null;
		}
	}
}

using System;
using System.Collections.Generic;

namespace TerrariaOverhaul.Common.ModEntities.Items.Overhauls
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
	public sealed class ItemAttachmentAttribute : Attribute
	{
		public readonly IReadOnlyList<int> ItemIds;

		public ItemAttachmentAttribute(params int[] itemIds)
		{
			ItemIds = Array.AsReadOnly(itemIds);
		}
	}
}

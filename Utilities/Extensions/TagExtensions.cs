using System;
using System.Collections.Generic;
using Terraria.ModLoader.Tags;

namespace TerrariaOverhaul.Utilities.Extensions
{
	public static class TagExtensions
	{
		public static void Populate(this TagData tag, int length, Func<int, bool> predicate)
		{
			for (int i = 0; i < length; i++) {
				if (predicate(i)) {
					tag.Set(i, true);
				}
			}
		}

		public static void PopulateFromSets(this TagData tag, bool[] sets)
		{
			for (int i = 0; i < sets.Length; i++) {
				if (sets[i]) {
					tag.Set(i, true);
				}
			}
		}

		public static void SetMultiple(this TagData tag, params int[] ids) => tag.SetMultiple((IReadOnlyList<int>)ids);
		public static void SetMultiple(this TagData tag, IReadOnlyList<int> ids)
		{
			for (int i = 0; i < ids.Count; i++) {
				tag.Set(ids[i], true);
			}
		}
	}
}

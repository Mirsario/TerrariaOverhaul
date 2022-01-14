using System;
using System.Collections.Generic;
using System.Diagnostics;
using Terraria;

namespace TerrariaOverhaul.Common.ModEntities.Items.Components
{
	public static class ItemComponentExtensions
	{
		public static T EnableComponent<T>(this Item item, Action<T> initializer = null) where T : ItemComponent
		{
			var component = item.GetGlobalItem<T>();

			component.Enabled = true;

			initializer?.Invoke(component);

			return component;
		}
	}
}

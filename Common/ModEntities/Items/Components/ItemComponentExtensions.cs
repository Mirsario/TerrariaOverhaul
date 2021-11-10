using System;
using Terraria;

namespace TerrariaOverhaul.Common.ModEntities.Items.Components
{
	public static class ItemComponentExtensions
	{
		public static T AddComponent<T>(this Item item, Action<T> initializer = null) where T : ItemComponent
		{
			var component = item.GetGlobalItem<T>();

			if (component.Enabled) {
				throw new InvalidOperationException($"Component {typeof(T).Name} is already enabled on item {item.Name}.");
			}

			component.Enabled = true;

			initializer?.Invoke(component);

			return component;
		}
	}
}

using System;
using Terraria;

namespace TerrariaOverhaul.Core.ItemComponents;

public static class ItemComponentExtensions
{
	public static T EnableComponent<T>(this Item item, Action<T>? initializer = null) where T : ItemComponent
	{
		var component = item.GetGlobalItem<T>();

		component.SetEnabled(item, true);

		initializer?.Invoke(component);

		return component;
	}
}

// Copyright (c) 2020-2025 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using System;
using Terraria;

namespace TerrariaOverhaul.Core.ItemComponents;

internal static class ItemComponentExtensions
{
	public static T EnableComponent<T>(this Item item, Action<T>? initializer = null) where T : ItemComponent
	{
		var component = item.GetGlobalItem<T>();

		component.SetEnabled(item, true);

		initializer?.Invoke(component);

		return component;
	}
}

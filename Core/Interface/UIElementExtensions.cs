using System;
using System.Linq;
using Terraria.ModLoader.UI.Elements;
using Terraria.UI;

namespace TerrariaOverhaul.Core.Interface;

public static class UIElementExtensions
{
	public static T With<T>(this T element, Action<T> action) where T : UIElement
	{
		action(element);

		return element;
	}

	public static T AddElement<T>(this UIElement parent, T child) where T : UIElement
	{
		if (parent is UIGrid uiGrid) {
			uiGrid.Add(child);
		} else {
			parent.Append(child);
		}

		return child;
	}

	public static T AddComponent<T>(this UIElement parent, T component) where T : UIComponent
	{
		component.AttachTo(parent);

		return component;
	}

	public static UIElement? GetFirstChildAt<T>(this UIElement parent, int level, Func<UIElement, bool> predicate) where T : UIElement
	{
		UIElement element = parent;

		for (int i = 0; i < level; i++) {
			if (predicate(element)) {
				return element;
			}

			element = element.Children.First();
		}

		return null;
	}
}

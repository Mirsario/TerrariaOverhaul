using System;
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
		parent.Append(child);

		return child;
	}
}

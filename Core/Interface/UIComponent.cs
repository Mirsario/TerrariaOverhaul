using System;
using Terraria.UI;

namespace TerrariaOverhaul.Core.Interface;

public abstract class UIComponent
{
	public UIElement Element { get; private set; } = null!;

	protected abstract void OnAttach();

	protected abstract void OnDetach();

	public void AttachTo(UIElement parent)
	{
		if (Element != null) {
			throw new InvalidOperationException("UI component already attached to an element.");
		}

		Element = parent;

		OnAttach();
	}

	public void Detach()
	{
		if (Element == null) {
			throw new InvalidOperationException("UI component not attached to an element.");
		}

		OnDetach();

		Element = null;
	}
}

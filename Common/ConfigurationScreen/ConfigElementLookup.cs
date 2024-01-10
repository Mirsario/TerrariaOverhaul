using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Terraria.UI;
using TerrariaOverhaul.Core.Configuration;

namespace TerrariaOverhaul.Common.ConfigurationScreen;

public static class ConfigElementLookup
{
	public delegate UIElement Constructor();

	private static readonly Dictionary<Type, Constructor> constructorByEntryType = new();

	static ConfigElementLookup()
	{
		// Bool
		Register<ConfigEntry<bool>, ToggleElement>();
		// Range
		Register<RangeConfigEntry<float>, RangeElement>();
		Register<RangeConfigEntry<double>, RangeElement>();
	}

	public static void Register<TEntry, TElement>()
		where TEntry : IConfigEntry
		where TElement : UIElement, IConfigEntryController, new()
	{
		Register<TEntry>(static () => new TElement());
	}

	public static void Register<TEntry>(Constructor elementConstructor)
		where TEntry : IConfigEntry
	{
		constructorByEntryType[typeof(TEntry)] = elementConstructor ?? throw new ArgumentNullException(nameof(elementConstructor));
	}

	public static UIElement CreateElement(IConfigEntry configEntry)
	{
		if (!TryCreateElement(configEntry, out var result)) {
			throw new InvalidOperationException($"No config element defined for config entry type '{configEntry.GetType().Name}'.");
		}

		return result;
	}

	public static bool TryCreateElement(IConfigEntry configEntry, [NotNullWhen(true)] out UIElement? result)
	{
		var entryType = configEntry.GetType();

		if (!constructorByEntryType.TryGetValue(entryType, out var constructor)) {
			result = null;

			return false;
		}

		result = constructor();

		if (result == null) {
			throw new InvalidOperationException($"Config element constructor for entry type '{entryType.Name}' has returned a null value.");
		}

		return true;
	}
}

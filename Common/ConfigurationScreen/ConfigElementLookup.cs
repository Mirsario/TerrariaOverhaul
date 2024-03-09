using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Terraria.UI;
using TerrariaOverhaul.Core.Configuration;

namespace TerrariaOverhaul.Common.ConfigurationScreen;

public static class ConfigElementLookup
{
	public delegate UIElement Constructor<TEntry>(TEntry entry) where TEntry : IConfigEntry;

	private static readonly Dictionary<Type, Constructor<IConfigEntry>> constructorByEntryType = new();

	static ConfigElementLookup()
	{
		// Bool
		Register<ConfigEntry<bool>>(_ => new ToggleElement());
		// Range
		Register<RangeConfigEntry<float>>(e => new RangeElement(e.MinValue, e.MaxValue));
		Register<RangeConfigEntry<double>>(e => new RangeElement(e.MinValue, e.MaxValue));
	}

	public static void Register<TEntry>(Constructor<TEntry> elementConstructor)
		where TEntry : IConfigEntry
	{
		if (elementConstructor == null) {
			throw new ArgumentNullException(nameof(elementConstructor));
		}

		// Type-erased
		constructorByEntryType[typeof(TEntry)] = Unsafe.As<Constructor<IConfigEntry>>(elementConstructor);
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

		result = constructor(configEntry);

		if (result == null) {
			throw new InvalidOperationException($"Config element constructor for entry type '{entryType.Name}' has returned a null value.");
		}

		return true;
	}
}

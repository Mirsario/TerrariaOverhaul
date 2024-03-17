using System;

namespace TerrariaOverhaul.Common.ConfigurationScreen;

public interface IConfigEntryController
{
	object? Value { get; set; }

	event Action? OnModified;
}

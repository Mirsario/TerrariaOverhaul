using System;
using Terraria.Localization;
using Terraria.ModLoader;

namespace TerrariaOverhaul.Core.Configuration;

public interface IConfigEntry
{
	Type ValueType { get; }
	string Name { get; }
	string Category { get; }
	string[] ExtraCategories { get; }
	object? Value { get; set; }
	object? LocalValue { get; set; }
	object? RemoteValue { get; set; }
	object DefaultValue { get; }
	ConfigSide Side { get; }
	public LocalizedText? DisplayName { get; }
	public LocalizedText? Description { get; }

	void Initialize(Mod mod);
}

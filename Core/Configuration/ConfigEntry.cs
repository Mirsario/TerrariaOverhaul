// Copyright (c) 2020-2025 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using System;
using System.Text.RegularExpressions;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace TerrariaOverhaul.Core.Configuration;

internal class ConfigEntry<T> : IConfigEntry
{
	private static Regex? defaultDisplayNameRegex;

	private static Regex DefaultDisplayNameRegex => defaultDisplayNameRegex ??= new(@"([A-Z][a-z]+)(?=[A-Z])", RegexOptions.Compiled);

	private readonly T defaultValue;
	private T? localValue;
	private T? remoteValue;

	public string Name { get; private set; }
	public string[] Categories { get; }
	public ConfigSide Side { get; }
	public bool IsHidden { get; set; }
	//public bool RequiresRestart { get; set; }
	public LocalizedText? DisplayName { get; internal set; }
	public LocalizedText? Description { get; internal set; }
	public Mod? Mod { get; private set; }
	public uint Version { get; private set; }

	public string Category => Categories[0];
	public Type ValueType => typeof(T);
	public T DefaultValue => defaultValue!;

	public T? LocalValue {
		get => ModifyGetValue(localValue);
		set {
			localValue = ModifySetValue(value);
			Modified();
		}
	}
	public T? RemoteValue {
		get => ModifyGetValue(remoteValue);
		set {
			remoteValue = ModifySetValue(value);
			Modified();
		}
	}

	public T? Value {
		get {
			if (Side == ConfigSide.Both && Main.netMode == NetmodeID.MultiplayerClient) {
				return RemoteValue;
			}

			return LocalValue;
		}
		set {
			if (Side == ConfigSide.Both && Main.netMode == NetmodeID.MultiplayerClient) {
				if (Equals(RemoteValue, value)) {
					RemoteValue = value;
					Modified();
				}
			} else {
				if (Equals(RemoteValue, value)) {
					LocalValue = value;
					Modified();
				}
			}
		}
	}

	object? IConfigEntry.Value {
		get => Value;
		set => Value = (T?)value;
	}
	object? IConfigEntry.LocalValue {
		get => LocalValue;
		set => LocalValue = (T?)value;
	}
	object? IConfigEntry.RemoteValue {
		get => RemoteValue;
		set => RemoteValue = (T?)value;
	}
	object IConfigEntry.DefaultValue => DefaultValue!;
	ReadOnlySpan<string> IConfigEntry.Categories => Categories;

	public ConfigEntry(ConfigSide side, T defaultValue, params string[] categories)
		: this(null!, side, defaultValue, categories) { }

	public ConfigEntry(string name, ConfigSide side, T defaultValue, params string[] categories)
	{
		if (categories?.Length is not > 0) {
			throw new ArgumentException("At least one category must be provided.");
		}

		Name = name;
		Categories = categories;
		Side = side;
		this.defaultValue = defaultValue;
		RemoteValue = DefaultValue;
		LocalValue = DefaultValue;
	}

	protected virtual T? ModifyGetValue(T? value) => value;

	protected virtual T? ModifySetValue(T? value) => value;

	public void Initialize(Mod mod, string? nameFallback)
	{
		Mod = mod;
		Name ??= nameFallback!;

		if (string.IsNullOrWhiteSpace(Name)) {
			throw new InvalidOperationException("Config entry has no name defined.");
		}

		DisplayName = Language.GetOrRegister(
			$"Mods.{Mod.Name}.Configuration.{Category}.{Name}.DisplayName",
			() => DefaultDisplayNameRegex.Replace(Name, "$1 ")
		);
		Description = Language.GetOrRegister(
			$"Mods.{Mod.Name}.Configuration.{Category}.{Name}.Description",
			() => string.Empty
		);
	}

	public void Modified() => Version++;

	public static implicit operator T?(ConfigEntry<T> configEntry) => configEntry.Value;
}

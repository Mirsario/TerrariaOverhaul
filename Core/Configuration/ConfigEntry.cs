using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace TerrariaOverhaul.Core.Configuration;

public class ConfigEntry<T> : IConfigEntry
{
	private readonly Func<T> defaultValueGetter;

	private T? localValue;
	private T? remoteValue;

	public string Name { get; }
	public string Category { get; }
	public ConfigSide Side { get; }
	//TODO: Make use of this in the yet to be made GUIs.
	public bool RequiresRestart { get; set; }
	public ModTranslation? DisplayName { get; internal set; }
	public ModTranslation? Description { get; internal set; }
	public Mod? Mod { get; private set; }

	public Type ValueType => typeof(T);
	public T DefaultValue => defaultValueGetter();

	public T? LocalValue {
		get => ModifyGetValue(localValue);
		set => localValue = ModifySetValue(value);
	}
	public T? RemoteValue {
		get => ModifyGetValue(remoteValue);
		set => remoteValue = ModifySetValue(value);
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
				RemoteValue = value;
			} else {
				LocalValue = value;
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

	public ConfigEntry(ConfigSide side, string category, string name, Func<T> defaultValueGetter)
	{
		Name = name;
		Category = category;
		Side = side;
		this.defaultValueGetter = defaultValueGetter;
		RemoteValue = DefaultValue;
		LocalValue = DefaultValue;

		ConfigSystem.RegisterEntry(this);
	}

	protected virtual T? ModifyGetValue(T? value) => value;

	protected virtual T? ModifySetValue(T? value) => value;

	public void Initialize(Mod mod)
	{
		Mod = mod;
		DisplayName = LocalizationLoader.CreateTranslation(mod, $"Configuration.{Category}.{Name}.DisplayName");
		Description = LocalizationLoader.CreateTranslation(mod, $"Configuration.{Category}.{Name}.Description");
	}

	public static implicit operator T?(ConfigEntry<T> configEntry) => configEntry.Value;
}

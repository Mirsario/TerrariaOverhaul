using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace TerrariaOverhaul.Core.Systems.Configuration
{
	public interface IConfigEntry
	{
		public string Id { get; }
		public object Value { get; set; }
		public object LocalValue { get; set; }

		void Initialize(Mod mod);
	}

	public sealed class ConfigEntry<T> : IConfigEntry
	{
		private readonly Func<T> DefaultValueGetter;

		private T remoteValue;
		private T localValue;

		public string Id { get; }
		public bool IsClientside { get; }
		public T LocalValue { get; set; }
		public ModTranslation DisplayName { get; internal set; }
		public ModTranslation Description { get; internal set; }
		public Mod Mod { get; private set; }

		public T DefaultValue => DefaultValueGetter();

		public T Value {
			get {
				if(!IsClientside && Main.netMode == NetmodeID.MultiplayerClient) {
					return remoteValue;
				}

				return localValue;
			}
			set {
				if(!IsClientside && Main.netMode == NetmodeID.MultiplayerClient) {
					remoteValue = value;
				} else {
					localValue = value;
				}
			}
		}

		object IConfigEntry.Value {
			get => Value;
			set => Value = (T)value;
		}
		object IConfigEntry.LocalValue {
			get => LocalValue;
			set => LocalValue = (T)value;
		}

		public ConfigEntry(string id, bool isClientside, Func<T> defaultValueGetter)
		{
			Id = id;
			IsClientside = isClientside;
			DefaultValueGetter = defaultValueGetter;
			Value = DefaultValue;

			ConfigSystem.ConfigEntries.Add(id, this);
		}

		public void Initialize(Mod mod)
		{
			DisplayName = mod.CreateTranslation($"Mods.{mod.Name}.Configuration.{Id}.Name");
			Description = mod.CreateTranslation($"Mods.{mod.Name}.Configuration.{Id}.Description");
		}
	}
}

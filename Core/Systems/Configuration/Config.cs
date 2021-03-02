using System;
using Newtonsoft.Json;
using Terraria.ModLoader;

namespace TerrariaOverhaul.Core.Systems.Configuration
{
	public abstract class Config : ILoadable, ICloneable
	{
		[JsonIgnore]
		public virtual string Name => GetType().Name;

		[JsonIgnore]
		public Config Local { get; internal set; }

		[JsonIgnore]
		public Config Server { get; internal set; }

		void ILoadable.Load(Mod mod)
		{
			Local = this;

			ConfigSystem.Configs.Add(this);
		}
		void ILoadable.Unload()
		{
			Local = null;
		}

		public object Clone() => MemberwiseClone();
	}
}

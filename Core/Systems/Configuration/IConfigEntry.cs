using System;
using Terraria.ModLoader;

namespace TerrariaOverhaul.Core.Systems.Configuration
{
	public interface IConfigEntry
	{
		public Type ValueType { get; }
		public string Name { get; }
		public string Category { get; }
		public object Value { get; set; }
		public object LocalValue { get; set; }

		void Initialize(Mod mod);
	}
}

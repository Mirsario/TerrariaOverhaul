using System;
using Terraria.ModLoader;

namespace TerrariaOverhaul.Common.Changes
{
	public sealed class RemoveDamageRandomization : ILoadable
	{
		public void Load(Mod mod) => On.Terraria.Main.DamageVar += (orig, damage, luck) => (int)Math.Round(damage);
		public void Unload() { }
	}
}

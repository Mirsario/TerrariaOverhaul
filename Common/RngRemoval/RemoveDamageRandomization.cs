using System;
using Terraria.ModLoader;

namespace TerrariaOverhaul.Common.RngRemoval;

public sealed class RemoveDamageRandomization : ILoadable
{
	public void Load(Mod mod) => On_Main.DamageVar += (orig, damage, luck) => (int)Math.Round(damage);
	public void Unload() { }
}

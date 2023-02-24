using System;
using Terraria;
using Terraria.ModLoader;

namespace TerrariaOverhaul.Common.RngRemoval;

public sealed class RemoveDamageRandomization : ILoadable
{
	public void Load(Mod mod)
	{
		On_Main.DamageVar += static (orig, damage, luck) => (int)Math.Round(damage);
	}

	public void Unload() { }
}

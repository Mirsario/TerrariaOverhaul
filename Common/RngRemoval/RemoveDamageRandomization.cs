using System;
using Terraria;
using Terraria.ModLoader;

namespace TerrariaOverhaul.Common.RngRemoval;

public sealed class RemoveDamageRandomization : ILoadable
{
	public void Load(Mod mod)
	{
		On_Main.DamageVar_float_int_float += DamageVarDetour;
	}

	public void Unload() { }

	private static int DamageVarDetour(On_Main.orig_DamageVar_float_int_float orig, float damage, int percent, float luck)
		=> (int)Math.Round(damage);
}

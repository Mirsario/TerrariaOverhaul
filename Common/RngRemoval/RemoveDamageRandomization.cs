// Copyright (c) 2020-2025 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using System;
using Terraria;
using Terraria.ModLoader;
using TerrariaOverhaul.Core.Configuration;

namespace TerrariaOverhaul.Common.RngRemoval;

public sealed class RemoveDamageRandomization : ILoadable
{
	public static readonly ConfigEntry<bool> EnableDamageRandomizationRemoval = new(ConfigSide.Both, true, "Balance");

	public void Load(Mod mod)
	{
		On_Main.DamageVar_float_int_float += DamageVarDetour;
	}

	public void Unload() { }

	private static int DamageVarDetour(On_Main.orig_DamageVar_float_int_float orig, float damage, int percent, float luck)
	{
		if (!EnableDamageRandomizationRemoval) {
			return orig(damage, percent, luck);
		}

		return (int)Math.Round(damage);
	}
}

// Copyright (c) 2020-2026 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using Terraria.Localization;
using Terraria.ModLoader;

namespace TerrariaOverhaul.Utilities.Terraria;

internal static class ModUtils
{
	public static string GetTextValue(this Mod mod, string key)
		=> Language.GetTextValue($"Mods.{mod.Name}.{key}");

	public static string GetTextValue(this Mod mod, string key, params object[] args)
		=> Language.GetTextValue($"Mods.{mod.Name}.{key}", args);
}

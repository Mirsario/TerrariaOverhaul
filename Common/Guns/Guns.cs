// Copyright (c) 2020-2026 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using TerrariaOverhaul.Core.Configuration;

namespace TerrariaOverhaul.Common.Guns;

internal static class Guns
{
	public static readonly ConfigEntry<bool> EnableGunSoundReplacements = new(ConfigSide.ClientOnly, true, "Guns");
	public static readonly ConfigEntry<bool> EnableAlternateGunFiringModes = new(ConfigSide.Both, true, "Guns");
}

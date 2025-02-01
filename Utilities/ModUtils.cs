// Copyright (c) 2020-2025 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using System;

namespace TerrariaOverhaul.Utilities;

public static class ModUtils
{
	public static string? GetTypePath(Type type)
		=> type.FullName?.Replace('.', '/');
}

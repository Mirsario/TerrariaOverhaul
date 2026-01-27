// Copyright (c) 2020-2026 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using System;

namespace TerrariaOverhaul.Core.Exceptions;

internal class OutdatedTModLoaderException : Exception
{
	public OutdatedTModLoaderException(Version minVersion) : base(GetMessage(minVersion)) { }

	public static string GetMessage(Version minVersion) => $"Please update your tModLoader to at least '{minVersion}'.";
}

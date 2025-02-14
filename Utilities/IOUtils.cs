// Copyright (c) 2020-2025 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Terraria.ModLoader;

namespace TerrariaOverhaul.Utilities;

public static class IOUtils
{
	// Operation Safety

	public static bool TryOperation<T>(Func<T> action, [NotNullWhen(true)] out T? result, int maxAttempts = 10, int sleepInterval = 50)
	{
		using var _ = new Logging.QuietExceptionHandle();

		for (int i = 0; i < maxAttempts; i++) {
			try {
				result = action()!;
				return true;
			}
			catch {
				Thread.Sleep(sleepInterval);
			}
		}

		result = default;
		return false;
	}
}

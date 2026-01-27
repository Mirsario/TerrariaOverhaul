// Copyright (c) 2020-2026 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using System;
using Microsoft.Build.Utilities;

namespace TerrariaOverhaul.BuildTools;

public abstract class TaskBase : Task
{
	public sealed override bool Execute()
	{
		try {
			Run();
		}
		catch (Exception e) {
			Log.LogErrorFromException(e, true);
		}

		return !Log.HasLoggedErrors;
	}

	protected abstract void Run();
}

// Copyright (c) 2020-2026 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using Terraria;
using Terraria.DataStructures;

#nullable enable

namespace TerrariaOverhaul.Common.EntitySources;

internal abstract class EntitySource_Gore : IEntitySource
{
	public Gore Gore { get; }
	public string? Context { get; }

	protected EntitySource_Gore(Gore gore, string? context = null)
	{
		Gore = gore;
		Context = context;
	}
}

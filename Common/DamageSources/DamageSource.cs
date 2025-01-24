// Copyright (c) 2020-2025 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using Terraria;

namespace TerrariaOverhaul.Common.DamageSources;

public sealed class DamageSource
{
	public readonly Entity Source;
	public readonly DamageSource? Parent;

	public DamageSource(Entity source, DamageSource? parent = null)
	{
		Source = source;
		Parent = parent;
	}
}

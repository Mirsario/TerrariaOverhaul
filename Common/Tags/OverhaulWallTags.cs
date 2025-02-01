// Copyright (c) 2020-2025 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using TerrariaOverhaul.Core.Tags;
using Group = TerrariaOverhaul.Core.Tags.WallTags;

namespace TerrariaOverhaul.Common.Tags;

public static class OverhaulWallTags
{
	/// <summary> Makes the wall flammable. </summary>
	public static readonly TagData Flammable = ContentTags.Get<Group>(nameof(Flammable));
}

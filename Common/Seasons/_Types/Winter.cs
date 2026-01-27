// Copyright (c) 2020-2026 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

#if ENABLE_SEASONS
namespace TerrariaOverhaul.Common.Seasons;

internal class Winter : Season
{
	protected internal override void Init()
	{
		Components.Add(new SnowSeasonComponent());
	}
}
#endif

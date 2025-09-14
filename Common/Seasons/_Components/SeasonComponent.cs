// Copyright (c) 2020-2025 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

#if ENABLE_SEASONS
using Terraria.ModLoader;
using TerrariaOverhaul.Core.Components;

namespace TerrariaOverhaul.Common.Seasons;

internal abstract class SeasonComponent : ModComponent<Season>
{
	public virtual void OnUpdate(Season season) { }

	public virtual void OnSeasonBegin(Season season) { }

	public virtual void OnSeasonEnd(Season season) { }

	public virtual void OnSeasonActivated(Season season) { }

	public virtual void OnSeasonDeactivated(Season season) { }

	protected override void Register()
	{
		base.Register();

		ModTypeLookup<SeasonComponent>.Register(this);
	}
}
#endif

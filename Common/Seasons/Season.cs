// Copyright (c) 2020-2025 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

#if ENABLE_SEASONS
using Microsoft.Xna.Framework;
using Terraria.ModLoader;
using TerrariaOverhaul.Core.Components;

namespace TerrariaOverhaul.Common.Seasons;

internal abstract class Season : ModType
{
	public readonly ModComponentContainer<Season, SeasonComponent> Components;

	public Color announceMessageColor;

	public int Id { get; internal set; }

	public bool IsActive => SeasonSystem.CurrentSeason.Id == Id;
	//public string DisplayName => LocalizationSystem.GetText($"SeasonSystem.Seasons.{Name}.Name");
	//public string AnnounceMessage
	//	=> LocalizationSystem.GetText($"SeasonSystem.Seasons.{Name}.Arrival{(WorldGen.crimson ? "Crimson" : "Corruption")}", null)
	//	?? LocalizationSystem.GetText($"SeasonSystem.Seasons.{Name}.Arrival");

	public Season()
	{
		Components = new(this);
	}

	protected internal virtual void Init() { }

	protected sealed override void Register()
	{

	}
}
#endif

using Terraria;
using TerrariaOverhaul.Core.Components;

namespace TerrariaOverhaul.Common.Seasons.Components
{
	[GlobalComponent]
	public sealed class ArrivalAnnouncementSeasonComponent : SeasonComponent
	{
		public override void OnSeasonBegin(Season season)
		{
			Main.NewText($"Season {season.Name} is here.");
		}
	}
}

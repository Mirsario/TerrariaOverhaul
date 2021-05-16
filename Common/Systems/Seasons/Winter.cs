using TerrariaOverhaul.Common.Systems.Seasons.Components;

namespace TerrariaOverhaul.Common.Systems.Seasons
{
	public class Winter : Season
	{
		protected internal override void Init()
		{
			Components.Add(new SnowSeasonComponent());
		}
	}
}

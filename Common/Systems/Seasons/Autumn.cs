using TerrariaOverhaul.Common.Systems.Seasons.Components;

namespace TerrariaOverhaul.Common.Systems.Seasons
{
	public class Autumn : Season
	{
		protected internal override void Init()
		{
			Components.Add(new GrassColorSeasonComponent());
		}
	}
}

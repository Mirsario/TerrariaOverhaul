namespace TerrariaOverhaul.Common.Seasons
{
	public class Winter : Season
	{
		protected internal override void Init()
		{
			Components.Add(new SnowSeasonComponent());
		}
	}
}

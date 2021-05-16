using Terraria.ModLoader;
using TerrariaOverhaul.Core.Components;

namespace TerrariaOverhaul.Common.Systems.Seasons.Components
{
	public abstract class SeasonComponent : ModComponent<Season>
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
}

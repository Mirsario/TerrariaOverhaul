using Terraria;
using Terraria.DataStructures;

namespace TerrariaOverhaul.Common.EntitySources
{
	public abstract class EntitySource_Gore : IEntitySource
	{
		public Gore Gore;

		protected EntitySource_Gore(Gore gore)
		{
			Gore = gore;
		}
	}
}

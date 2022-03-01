using Terraria;
using Terraria.DataStructures;

namespace TerrariaOverhaul.Common.EntitySources
{
	public class EntitySource_Entity : IEntitySource
	{
		public Entity Entity;

		public EntitySource_Entity(Entity entity)
		{
			Entity = entity;
		}
	}
}

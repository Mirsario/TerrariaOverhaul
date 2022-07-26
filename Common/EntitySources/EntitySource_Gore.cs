using Terraria;
using Terraria.DataStructures;

#nullable enable

namespace TerrariaOverhaul.Common.EntitySources;

public abstract class EntitySource_Gore : IEntitySource
{
	public Gore Gore { get; }
	public string? Context { get; }

	protected EntitySource_Gore(Gore gore, string? context = null)
	{
		Gore = gore;
		Context = context;
	}
}

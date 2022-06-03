using Terraria.ModLoader;

namespace TerrariaOverhaul.Core.Tags
{
	// Horrible. Taken from an old 1.4_contenttags draft of TML. And made worse.
	
	public sealed class ItemTags : TagGroup
	{
		public override int TypeCount => ItemLoader.ItemCount;
	}

	public sealed class NPCTags : TagGroup
	{
		public override int TypeCount => NPCLoader.NPCCount;
	}

	public sealed class ProjectileTags : TagGroup
	{
		public override int TypeCount => ProjectileLoader.ProjectileCount;
	}

	public sealed class TileTags : TagGroup
	{
		public override int TypeCount => TileLoader.TileCount;
	}

	public sealed class WallTags : TagGroup
	{
		public override int TypeCount => WallLoader.WallCount;
	}
}

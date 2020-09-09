using Terraria.ModLoader.Tags;
using Group = Terraria.ModLoader.Tags.WallTags;

namespace TerrariaOverhaul.Common.Tags
{
	public static class OverhaulWallTags
	{
		/// <summary> Makes the wall flammable. </summary>
		public static readonly TagData Flammable = ContentTags.Get<Group>(nameof(Flammable));
	}
}

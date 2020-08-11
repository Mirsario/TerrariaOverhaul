using Terraria.ModLoader.Tags;

namespace TerrariaOverhaul.Common.Tags.Tiles
{
	/// <summary> Tiles with this tag will not spread fire onto other blocks. Used for things that never actually get destroyed by fire, like bushes. </summary>
	public sealed class TileNoFireSpreadingTag : TagShortcut<TileTags>
	{
		public override string TagName => "nofirespreading";
	}
}

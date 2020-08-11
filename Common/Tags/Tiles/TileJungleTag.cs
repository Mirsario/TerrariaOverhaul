using Terraria.ModLoader.Tags;

namespace TerrariaOverhaul.Common.Tags.Tiles
{
	/// <summary> Bee hives never fall on it's own when close to this tile. </summary>
	public sealed class TileJungleTag : TagShortcut<TileTags>
	{
		public override string TagName => "jungle";
	}
}

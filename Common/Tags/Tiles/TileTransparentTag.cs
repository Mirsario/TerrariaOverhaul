using Terraria.ModLoader.Tags;

namespace TerrariaOverhaul.Common.Tags.Tiles
{
	/// <summary> Monsters can spot players through this tile. </summary>
	public sealed class TileTransparentTag : TagShortcut<TileTags>
	{
		public override string TagName => "transparent";
	}
}

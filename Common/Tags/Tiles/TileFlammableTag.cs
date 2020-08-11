using Terraria.ModLoader.Tags;

namespace TerrariaOverhaul.Common.Tags.Tiles
{
	/// <summary> Makes the tile flammable. </summary>
	public sealed class TileFlammableTag : TagShortcut<TileTags>
	{
		public override string TagName => "flammable";
	}
}

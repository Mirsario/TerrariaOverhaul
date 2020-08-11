using Terraria.ModLoader.Tags;

namespace TerrariaOverhaul.Common.Tags.Tiles
{
	/// <summary> Raises temperature, automatically results in <see cref="TileNoBeeHives"/> tag being added. </summary>
	public sealed class TileHeatSourceTag : TagShortcut<TileTags>
	{
		public override string TagName => "heatsource";
	}
}

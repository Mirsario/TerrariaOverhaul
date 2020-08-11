using Terraria.ModLoader.Tags;

namespace TerrariaOverhaul.Common.Tags.Tiles
{
	/// <summary> Prevents water evaporation around this block. </summary>
	public sealed class TileColdTag : TagShortcut<TileTags>
	{
		public override string TagName => "cold";
	}
}

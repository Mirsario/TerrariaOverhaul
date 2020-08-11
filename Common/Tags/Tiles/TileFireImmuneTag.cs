using Terraria.ModLoader.Tags;

namespace TerrariaOverhaul.Common.Tags.Tiles
{
	/// <summary> Tiles with this tag will not be destroyed by fire. Used by bushes & trees. </summary>
	public sealed class TileFireImmuneTag : TagShortcut<TileTags>
	{
		public override string TagName => "fireimmune";
	}
}

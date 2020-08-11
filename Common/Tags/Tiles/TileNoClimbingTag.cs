using Terraria.ModLoader.Tags;

namespace TerrariaOverhaul.Common.Tags.Tiles
{
	/// <summary> Disallows block climbing. Used for ice, usually. </summary>
	public sealed class TileNoClimbingTag : TagShortcut<TileTags>
	{
		public override string TagName => "allowclimbing";
	}
}

using Terraria.ModLoader.Tags;

namespace TerrariaOverhaul.Common.Tags.Tiles
{
	/// <summary> Makes fire activate tiles like dynamite in wiring. </summary>
	public sealed class TileFireActivatedTag : TagShortcut<TileTags>
	{
		public override string TagName => "fireactivated";
	}
}

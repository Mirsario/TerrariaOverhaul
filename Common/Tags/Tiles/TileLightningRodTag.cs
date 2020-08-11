using Terraria.ModLoader.Tags;

namespace TerrariaOverhaul.Common.Tags.Tiles
{
	/// <summary> Like metallic, but *always* attracts lightning. </summary>
	public sealed class TileLightningRodTag : TagShortcut<TileTags>
	{
		public override string TagName => "lightningrod";
	}
}

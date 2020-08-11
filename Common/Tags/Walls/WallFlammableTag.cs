using Terraria.ModLoader.Tags;

namespace TerrariaOverhaul.Common.Tags.Walls
{
	/// <summary> Makes the wall flammable. </summary>
	public sealed class WallFlammableTag : TagShortcut<WallTags>
	{
		public override string TagName => "flammable";
	}
}

using Terraria.ModLoader.Tags;
using Group = Terraria.ModLoader.Tags.TileTags;

namespace TerrariaOverhaul.Common.Tags
{
	public static class TileTags
	{
		/// <summary> Used for platforms. </summary>
		public static readonly TagData AllowClimbing = ContentTags.Get<Group>(nameof(AllowClimbing));

		/// <summary> Prevents water evaporation around this block. </summary>
		public static readonly TagData Cold = ContentTags.Get<Group>(nameof(Cold));

		/// <summary> Affects footstep sounds. </summary>
		public static readonly TagData Dirt = ContentTags.Get<Group>(nameof(Dirt));

		/// <summary> Makes fire activate tiles like dynamite in wiring. </summary>
		public static readonly TagData FireActivated = ContentTags.Get<Group>(nameof(FireActivated));

		/// <summary> Tiles with this tag will not be destroyed by fire. Used by bushes & trees. </summary>
		public static readonly TagData FireImmune = ContentTags.Get<Group>(nameof(FireImmune));

		/// <summary> Makes the tile flammable. </summary>
		public static readonly TagData Flammable = ContentTags.Get<Group>(nameof(Flammable));

		/// <summary> Affects footstep sounds. </summary>
		public static readonly TagData Grass = ContentTags.Get<Group>(nameof(Grass));

		/// <summary> Raises temperature, automatically results in <see cref="TileNoBeeHives"/> tag being added. </summary>
		public static readonly TagData HeatSource = ContentTags.Get<Group>(nameof(HeatSource));

		/// <summary> Bee hives never fall on it's own when close to this tile. </summary>
		public static readonly TagData Jungle = ContentTags.Get<Group>(nameof(Jungle));

		/// <summary> Like metallic, but *always* attracts lightning. </summary>
		public static readonly TagData LightningRod = ContentTags.Get<Group>(nameof(LightningRod));

		/// <summary> Makes tile attract lightning and conduct electricity. </summary>
		public static readonly TagData Metallic = ContentTags.Get<Group>(nameof(Metallic));

		/// <summary> Raises temperature, automatically results in <see cref="TileNoBeeHives"/> tag being added. </summary>
		public static readonly TagData NoClimbing = ContentTags.Get<Group>(nameof(NoClimbing));

		/// <summary> Tiles with this tag will not spread fire onto other blocks. Used for things that never actually get destroyed by fire, like bushes. </summary>
		public static readonly TagData NoFireSpreading = ContentTags.Get<Group>(nameof(NoFireSpreading));

		/// <summary> Affects footstep sounds. </summary>
		public static readonly TagData Sand = ContentTags.Get<Group>(nameof(Sand));

		/// <summary> Monsters can spot players through this tile. </summary>
		public static readonly TagData Transparent = ContentTags.Get<Group>(nameof(Transparent));
	}
}

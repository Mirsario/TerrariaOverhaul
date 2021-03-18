using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Tags;
using TerrariaOverhaul.Utilities.Extensions;
using Group = Terraria.ModLoader.Tags.ItemTags;

namespace TerrariaOverhaul.Common.Tags
{
	public sealed class OverhaulItemTags : ILoadable
	{
		/// <summary> Speeds up climbing. </summary>
		public static readonly TagData ClimbingClaws = ContentTags.Get<Group>(nameof(ClimbingClaws));

		/// <summary> Increases stomping damage. </summary>
		public static readonly TagData ShoeSpikes = ContentTags.Get<Group>(nameof(ShoeSpikes));

		/// <summary> Enables walljumping. </summary>
		public static readonly TagData NinjaGear = ContentTags.Get<Group>(nameof(NinjaGear));

		/// <summary> Affects hit sounds. </summary>
		public static readonly TagData Wooden = ContentTags.Get<Group>(nameof(Wooden));

		void ILoadable.Load(Mod mod)
		{
			ClimbingClaws.SetMultiple(
				ItemID.ClimbingClaws,
				ItemID.TigerClimbingGear,
				ItemID.MasterNinjaGear
			);

			ShoeSpikes.SetMultiple(
				ItemID.ShoeSpikes,
				ItemID.TigerClimbingGear,
				ItemID.MasterNinjaGear
			);

			NinjaGear.SetMultiple(
				ItemID.ClimbingClaws,
				ItemID.ShoeSpikes,
				ItemID.TigerClimbingGear,
				ItemID.MasterNinjaGear
			);

			Wooden.SetMultiple(
				ItemID.WoodenSword
			);
		}
		void ILoadable.Unload()
		{

		}
	}
}

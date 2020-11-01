using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Tags;
using TerrariaOverhaul.Utilities.Extensions;
using Group = Terraria.ModLoader.Tags.ItemTags;

namespace TerrariaOverhaul.Common.Tags
{
	public sealed class OverhaulProjectileTags : ILoadable
	{
		/// <summary> These set things on fire. Enough said. </summary>
		public static readonly TagData Incendiary = ContentTags.Get<Group>(nameof(Incendiary));

		/// <summary> Projectiles with this tag extinguish fires and interact with incendiary projectiles. </summary>
		public static readonly TagData Extinguisher = ContentTags.Get<Group>(nameof(Extinguisher));

		/// <summary> Grappling hooks with this tag won't have Overhaul's physics improvements. </summary>
		public static readonly TagData NoGrapplingHookSwinging = ContentTags.Get<Group>(nameof(NoGrapplingHookSwinging));

		void ILoadable.Load(Mod mod)
		{
			Incendiary.SetMultiple(
				ProjectileID.Spark,
				ProjectileID.FlamesTrap,
				ProjectileID.GreekFire1,
				ProjectileID.GreekFire2,
				ProjectileID.GreekFire3,
				ProjectileID.MolotovCocktail,
				ProjectileID.MolotovFire,
				ProjectileID.MolotovFire2,
				ProjectileID.MolotovFire3,
				ProjectileID.CultistBossFireBall,
				ProjectileID.DD2BetsyFireball,
				ProjectileID.DD2BetsyFlameBreath,
				ProjectileID.InfernoFriendlyBlast,
				ProjectileID.InfernoHostileBlast,
				ProjectileID.Fireball,
				ProjectileID.BallofFire,
				ProjectileID.Flamelash,
				ProjectileID.Flames,
				ProjectileID.FireArrow,
				ProjectileID.Meteor1,
				ProjectileID.Meteor2,
				ProjectileID.Meteor3,
				ProjectileID.Flare
			);

			Extinguisher.SetMultiple(
				ProjectileID.WaterStream,
				ProjectileID.WaterBolt,
				ProjectileID.HolyWater,
				ProjectileID.UnholyWater,
				ProjectileID.BloodWater,
				ProjectileID.WaterGun,
				ProjectileID.CultistBossIceMist,
				ProjectileID.IceBolt,
				ProjectileID.IceBlock,
				ProjectileID.IceBoomerang,
				ProjectileID.IceSpike,
				ProjectileID.IcewaterSpit,
				ProjectileID.IceSickle,
				ProjectileID.BallofFrost,
				ProjectileID.FrostArrow,
				ProjectileID.FrostBeam,
				ProjectileID.FrostBlastFriendly,
				ProjectileID.FrostBlastHostile,
				ProjectileID.FrostBoltStaff,
				ProjectileID.FrostBoltSword,
				ProjectileID.FrostburnArrow,
				ProjectileID.FrostDaggerfish,
				ProjectileID.FrostHydra,
				ProjectileID.FrostShard,
				ProjectileID.FrostWave,
				ProjectileID.GoldenShowerFriendly,
				ProjectileID.GoldenShowerHostile,
				ProjectileID.BabySnowman,
				ProjectileID.NorthPoleSnowflake,
				ProjectileID.SnowBallFriendly,
				ProjectileID.SnowBallHostile,
				ProjectileID.BlueFlare,
				ProjectileID.BloodRain,
				ProjectileID.BloodCloudMoving,
				ProjectileID.BloodCloudRaining,
				ProjectileID.BloodWater,
				ProjectileID.RainCloudMoving,
				ProjectileID.RainCloudRaining,
				ProjectileID.RainFriendly,
				ProjectileID.RainNimbus
			);

			NoGrapplingHookSwinging.SetMultiple(
				ProjectileID.QueenSlimeHook,
				ProjectileID.AntiGravityHook,
				ProjectileID.StaticHook
			);
		}
		void ILoadable.Unload() { }
	}
}

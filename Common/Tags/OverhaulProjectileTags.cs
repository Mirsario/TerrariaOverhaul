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

		/// <summary> Adds screenshake and extra knockback when this projectile is killed. </summary>
		public static readonly TagData Explosive = ContentTags.Get<Group>(nameof(Explosive));

		/// <summary> Changes audio on tile collision. </summary>
		public static readonly TagData Bullet = ContentTags.Get<Group>(nameof(Bullet));

		/// <summary> Used in determining whether something's a rocket launcher or a grenade launcher. </summary>
		public static readonly TagData Rocket = ContentTags.Get<Group>(nameof(Rocket));

		/// <summary> Used in determining whether something's a rocket launcher or a grenade launcher. </summary>
		public static readonly TagData Grenade = ContentTags.Get<Group>(nameof(Grenade));

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

			Explosive.SetMultiple(
				ProjectileID.BlackBolt,
				ProjectileID.Bomb,
				ProjectileID.BouncyBomb,
				ProjectileID.StickyBomb,
				ProjectileID.Grenade,
				ProjectileID.BouncyGrenade,
				ProjectileID.StickyGrenade,
				ProjectileID.RocketI,
				ProjectileID.RocketII,
				ProjectileID.RocketIII,
				ProjectileID.RocketIV,
				ProjectileID.GrenadeI,
				ProjectileID.GrenadeII,
				ProjectileID.GrenadeIII,
				ProjectileID.GrenadeIV,
				ProjectileID.NailFriendly,
				ProjectileID.HellfireArrow,
				ProjectileID.Dynamite,
				ProjectileID.BouncyDynamite,
				ProjectileID.StickyDynamite,
				ProjectileID.BombFish,
				ProjectileID.Meteor1,
				ProjectileID.Meteor2,
				ProjectileID.Meteor3,
				ProjectileID.ExplosiveBullet,
				ProjectileID.FallingStar
			);

			Bullet.SetMultiple(
				ProjectileID.Bullet,
				ProjectileID.BulletHighVelocity,
				ProjectileID.BulletSnowman,
				ProjectileID.BulletDeadeye,
				ProjectileID.ChlorophyteBullet,
				ProjectileID.CrystalBullet,
				ProjectileID.CursedBullet,
				ProjectileID.ExplosiveBullet,
				ProjectileID.GoldenBullet,
				ProjectileID.IchorBullet,
				ProjectileID.MoonlordBullet,
				ProjectileID.NanoBullet,
				ProjectileID.PartyBullet,
				ProjectileID.SniperBullet,
				ProjectileID.VenomBullet,
				ProjectileID.MeteorShot
			);

			Rocket.SetMultiple(
				ProjectileID.RocketI,
				ProjectileID.RocketII,
				ProjectileID.RocketIII,
				ProjectileID.RocketIV
			);

			Grenade.SetMultiple(
				ProjectileID.Grenade,
				ProjectileID.BouncyGrenade,
				ProjectileID.StickyGrenade,
				ProjectileID.GrenadeI,
				ProjectileID.GrenadeII,
				ProjectileID.GrenadeIII,
				ProjectileID.GrenadeIV
			);
		}
		void ILoadable.Unload() { }
	}
}

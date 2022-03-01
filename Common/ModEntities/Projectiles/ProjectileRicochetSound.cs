﻿using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.Tags;

namespace TerrariaOverhaul.Common.ModEntities.Projectiles
{
	[Autoload(Side = ModSide.Client)]
	public class ProjectileRicochetSound : GlobalProjectile
	{
		public static readonly ModSoundStyle RicochetSound = new($"{nameof(TerrariaOverhaul)}/Assets/Sounds/HitEffects/Ricochet", 2, volume: 0.1f);

		public override bool AppliesToEntity(Projectile projectile, bool lateInstantiation)
			=> OverhaulProjectileTags.Bullet.Has(projectile.type);

		public override bool OnTileCollide(Projectile projectile, Vector2 oldVelocity)
		{
			SoundEngine.PlaySound(RicochetSound, projectile.Center);

			return true;
		}
	}
}

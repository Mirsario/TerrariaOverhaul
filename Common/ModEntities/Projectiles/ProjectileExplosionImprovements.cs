using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.AudioEffects;
using TerrariaOverhaul.Common.Camera.ScreenShakes;
using TerrariaOverhaul.Common.Decals;
using TerrariaOverhaul.Common.Tags;
using TerrariaOverhaul.Utilities;
using TerrariaOverhaul.Utilities.Extensions;
using TerrariaOverhaul.Common.Gores;
using TerrariaOverhaul.Core.Time;

namespace TerrariaOverhaul.Common.ModEntities.Projectiles
{
	public class ProjectileExplosionImprovements : GlobalProjectile
	{
		private Vector2 maxSize;

		public override bool InstancePerEntity => true;

		public override bool AppliesToEntity(Projectile projectile, bool lateInstantiation)
			=> OverhaulProjectileTags.Explosive.Has(projectile.type);

		public override bool PreAI(Projectile projectile)
		{
			maxSize = Vector2.Max(maxSize, projectile.Size);

			return true;
		}

		public override void Kill(Projectile projectile, int timeLeft)
		{
			maxSize = Vector2.Max(maxSize, projectile.Size);

			if (maxSize.X <= 0f || maxSize.Y <= 0f) {
				return;
			}

			float maxPower = (float)Math.Sqrt(maxSize.X * maxSize.Y);

			//TODO: Hardcoded cuz tired.
			if (projectile.type == ProjectileID.ExplosiveBullet) {
				maxPower = 10f;
			}

			float knockbackRange = maxPower;
			float knockbackRangeSquared = knockbackRange * knockbackRange;
			var center = projectile.Center;

			// Knockback
			for (int i = 0; i < Main.maxPlayers + Main.maxItems + Main.maxGore; i++) {
				ref Vector2 velocity = ref projectile.velocity; // Unused assignment.
				Rectangle rectangle;
				object entity;

				if (i < Main.maxPlayers) {
					var player = Main.player[i];

					if (player?.active != true) {
						continue;
					}

					entity = player;
					velocity = ref player.velocity;
					rectangle = player.getRect();
				} else if (i < Main.maxPlayers + Main.maxItems) {
					var item = Main.item[i - Main.maxPlayers];

					if (item?.active != true) {
						continue;
					}

					entity = item;
					velocity = ref item.velocity;
					rectangle = item.getRect();
				} else {
					var gore = Main.gore[i - Main.maxPlayers - Main.maxItems];

					if (gore?.active != true) {
						continue;
					}

					entity = gore;
					velocity = ref gore.velocity;
					rectangle = gore.AABBRectangle;
				}

				float sqrDistance = Vector2.DistanceSquared(rectangle.GetCorner(center), center);

				if (sqrDistance >= knockbackRangeSquared) {
					continue;
				}

				var direction = (rectangle.Center() - center).SafeNormalize(default);
				float distance = (float)Math.Sqrt(sqrDistance);

				if (float.IsNaN(distance) || direction == default) {
					continue;
				}

				// Explosions have a chance to set gore on fire.
				if (entity is OverhaulGore goreExt && Main.rand.Next(5) == 0) {
					goreExt.onFire = true;
				}

				velocity += direction * MathUtils.DistancePower(distance, knockbackRange) * maxPower / 13f;

				if (velocity.HasNaNs()) {
					velocity = Vector2.Zero;
				}
			}

			if (!Main.dedServ) {
				if (Main.LocalPlayer != null) {
					float distance = Vector2.Distance(Main.LocalPlayer.Center, center);

					// Screenshake
					float screenshakePower = MathUtils.DistancePower(distance, maxPower * 10f) * 15f;

					if (screenshakePower > 0f) {
						ScreenShakeSystem.New(screenshakePower, 0.5f);
					}

					// Low-pass filtering
					int lowPassFilteringTime = (int)(TimeSystem.LogicFramerate * 5f * MathUtils.DistancePower(distance, maxPower * 3f));

					if (lowPassFilteringTime > 0) {
						AudioEffectsSystem.AddAudioEffectModifier(
							lowPassFilteringTime,
							$"{nameof(TerrariaOverhaul)}/{nameof(ProjectileExplosionImprovements)}",
							(float intensity, ref AudioEffectParameters soundParameters, ref AudioEffectParameters musicParameters) => {
								float total = intensity * 0.5f;

								soundParameters.LowPassFiltering += total;
								musicParameters.LowPassFiltering += total;
							}
						);
					}
				}

				// Decal
				var rect = new Rectangle((int)projectile.Center.X, (int)projectile.Center.Y, 0, 0);

				rect.Inflate(64, 64);

				DecalSystem.AddDecals(Mod.Assets.Request<Texture2D>("Assets/Textures/ExplosionDecal").Value, rect, new Color(255, 255, 255, (int)Math.Min(maxPower, 128)));
			}
		}
	}
}

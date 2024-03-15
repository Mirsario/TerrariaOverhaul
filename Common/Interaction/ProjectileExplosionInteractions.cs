using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.BloodAndGore;
using TerrariaOverhaul.Common.Tags;
using TerrariaOverhaul.Core.Debugging;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.Interaction;

public sealed class ProjectileExplosionInteractions : GlobalProjectile
{
	private Vector2Int maxSize;

	public bool Enabled { get; set; }
	public bool AffectsVisualEntities { get; set; } = true;
	public bool AffectsGameplayEntities { get; set; } = false;
	public bool SetsGoreOnFire { get; set; }
	public float? MinPower { get; set; }
	public float? MaxPower { get; set; }

	public override bool InstancePerEntity => true;

	public override void SetDefaults(Projectile projectile)
	{
		if (OverhaulProjectileTags.Bullet.Has(projectile.type)) {
			Enabled = true;
			AffectsGameplayEntities = false;
			MinPower = 25f;
		}

		if (OverhaulProjectileTags.Explosive.Has(projectile.type)) {
			Enabled = true;
			AffectsGameplayEntities = true;
			SetsGoreOnFire = true;

			if (projectile.type == ProjectileID.ExplosiveBullet) {
				MaxPower = 25f;
			}
		}
	}

	public override bool PreAI(Projectile projectile)
	{
		if (Enabled) {
			UpdateMaxSize(projectile);
		}

		return true;
	}

	public override void OnKill(Projectile projectile, int timeLeft)
	{
		if (!Enabled) {
			return;
		}

		UpdateMaxSize(projectile);

		if (maxSize.X <= 0f || maxSize.Y <= 0f) {
			return;
		}

		var center = projectile.Center;
		float power = MathHelper.Clamp(
			(float)Math.Sqrt(maxSize.X * maxSize.Y),
			MinPower ?? float.NegativeInfinity,
			MaxPower ?? float.PositiveInfinity
		);
		float range = power;
		float knockback = power / 13f;

		ApplySplashEffects(center, range, knockback);
	}

	private void ApplySplashEffects(Vector2 center, float range, float knockback)
	{
		float rangeSquared = range * range;

		if (!Main.dedServ && DebugSystem.EnableDebugRendering) {
			DebugSystem.DrawCircle(center, range, Color.OrangeRed);
		}

		if (AffectsGameplayEntities) {
			static void ApplyVelocity(Entity entity, Vector2 velocity)
				=> entity.velocity += velocity;

			foreach (var player in ActiveEntities.Players) {
				ApplySplashEffects(player, ApplyVelocity, player.GetRectangle(), center, range, rangeSquared, knockback);
			}

			foreach (var npc in ActiveEntities.NPCs) {
				ApplySplashEffects(npc, ApplyVelocity, npc.GetRectangle(), center, range, rangeSquared, knockback * npc.knockBackResist);
			}
		}

		if (AffectsVisualEntities) {
			static void ApplyVelocity(Gore entity, Vector2 velocity)
				=> entity.velocity += velocity;

			foreach (var gore in ActiveEntities.Gores) {
				if (gore is OverhaulGore g && g.Time == 0) {
					continue;
				}

				ApplySplashEffects(gore, ApplyVelocity, gore.AABBRectangle, center, range, rangeSquared, knockback);
			}
		}
	}

	private void ApplySplashEffects<T>(T entity, Action<T, Vector2> applyVelocityFunction, Rectangle entityAabb, Vector2 center, float range, float rangeSquared, float knockback)
	{
		float sqrDistance = Vector2.DistanceSquared(entityAabb.GetCorner(center), center);

		if (sqrDistance >= rangeSquared) {
			return;
		}

		var entityCenter = entityAabb.Center();
		var direction = (entityCenter - center).SafeNormalize(default);
		float distance = (float)Math.Sqrt(sqrDistance);

		if (float.IsNaN(distance) || direction == default) {
			return;
		}

		float distanceFactor = MathUtils.DistancePower(distance, range);
		var velocity = direction * distanceFactor * knockback;

		applyVelocityFunction(entity, velocity);

		if (entity is OverhaulGore gore) {
			gore.Damage();

			// Explosions have a chance to set gore on fire.
			if (SetsGoreOnFire && gore.BleedColor.HasValue && Main.rand.NextBool(5)) {
				gore.OnFire = true;
			}
		}
	}

	private void UpdateMaxSize(Projectile projectile)
	{
		maxSize = Vector2Int.Max(maxSize, new Vector2Int(projectile.width, projectile.height));
	}
}

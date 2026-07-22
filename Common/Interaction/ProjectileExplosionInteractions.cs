// Copyright (c) 2020-2026 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.BloodAndGore;
using TerrariaOverhaul.Core.Debugging;
using TerrariaOverhaul.Core.Tags;
using TerrariaOverhaul.Utilities.Terraria;
using TerrariaOverhaul.Utilities.Xna;

namespace TerrariaOverhaul.Common.Interaction;

internal sealed class ProjectileExplosionInteractions : GlobalProjectile
{
	private static readonly ContentSet Bullet = nameof(Bullet);
	private static readonly ContentSet Explosive = nameof(Explosive);
	private static readonly ContentSet AlwaysMovedByExplosions = nameof(AlwaysMovedByExplosions);

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
		if (Bullet.Has(projectile)) {
			Enabled = true;
			AffectsGameplayEntities = false;
			MinPower = 25f;
		}

		if (Explosive.Has(projectile)) {
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
				// Do not affect gores that have *just* been made.
				if (gore is OverhaulGore g && g.Time == 0) continue;

				// If this is a non-colliding gore, require an opt-in tag.
				if (!gore.sticky && !AlwaysMovedByExplosions.Has(gore)) continue;

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

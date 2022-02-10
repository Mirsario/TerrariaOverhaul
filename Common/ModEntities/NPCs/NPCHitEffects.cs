using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.Time;

namespace TerrariaOverhaul.Common.ModEntities.NPCs
{
	// Offsets, rotates, and scales enemies whenever they're hit, so that they look less static even when they're not moving.
	// Looks like some sort of flinching.
	public class NPCHitEffects : GlobalNPC
	{
		private const int EffectLength = 10;

		private ulong lastHitTime;
		private float? usedDrawScaleMultiplier;
		private float? usedDrawRotationOffset;
		private Vector2? usedDrawPositionOffset;

		public override bool InstancePerEntity => true;

		public override void OnHitByItem(NPC npc, Player player, Item item, int damage, float knockback, bool crit)
			=> ResetHitTime();

		public override void OnHitByProjectile(NPC npc, Projectile projectile, int damage, float knockback, bool crit)
			=> ResetHitTime();

		// Drawing
		public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			ulong delta = TimeSystem.UpdateCount - lastHitTime;

			const int EffectLength = 5;

			if (delta <= EffectLength) {
				float intensity = 1f - (delta / (float)EffectLength);
				float maxDimension = Math.Max(1f, Math.Max(npc.width, npc.height) * npc.scale);
				float maxScaleDown = maxDimension / (maxDimension + 4f);

				usedDrawScaleMultiplier = MathHelper.Lerp(1f, maxScaleDown, intensity);
				usedDrawRotationOffset = npc.direction * MathHelper.ToRadians(-(1000f / maxDimension)) * intensity;
				usedDrawPositionOffset = Main.rand.NextVector2Circular(2f, 2f) * intensity;

				npc.scale *= usedDrawScaleMultiplier.Value;
				npc.rotation += usedDrawRotationOffset.Value;
				npc.position += usedDrawPositionOffset.Value;
			}

			return true;
		}

		public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			if (usedDrawScaleMultiplier.HasValue) {
				if (usedDrawScaleMultiplier.Value != 0f) {
					npc.scale /= usedDrawScaleMultiplier.Value;
				}

				usedDrawScaleMultiplier = null;
			}

			if (usedDrawRotationOffset.HasValue) {
				npc.rotation -= usedDrawRotationOffset.Value;
				usedDrawRotationOffset = null;
			}

			if (usedDrawPositionOffset.HasValue) {
				npc.position -= usedDrawPositionOffset.Value;
				usedDrawPositionOffset = null;
			}
		}

		private void ResetHitTime()
		{
			lastHitTime = TimeSystem.UpdateCount;
		}
	}
}

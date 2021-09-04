using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.Hooks.Npcs;
using TerrariaOverhaul.Core.Systems.Debugging;

namespace TerrariaOverhaul.Common.ModEntities.NPCs.AI
{
	public sealed class NpcShootingPredictions : GlobalNPC, INpcModifyShootProjectile
	{
		public override bool AppliesToEntity(NPC entity, bool lateInstantiation)
			=> entity.ModNPC == null;

		public void ModifyShootProjectile(NPC npc, ref int type, ref Vector2 position, ref Vector2 velocity, ref int damage, ref float knockback)
		{
			if(velocity == default || velocity.HasNaNs()) {
				return;
			}

			if(!npc.TryGetGlobalNPC<NpcTargeting>(out var targeting)) {
				return;
			}

			targeting.ReactionFactor = 0.05f;
			targeting.ReactionDelayInTicks = 10;

			float shootSpeed = velocity.Length();
			var predictedPosition = targeting.GetPredictionWithSpeed(npc, shootSpeed);

			targeting.DebugProjectileSpeed = shootSpeed;

			velocity = (predictedPosition - position).SafeNormalize(Vector2.UnitX) * shootSpeed;

			if(npc.type == NPCID.TacticalSkeleton) {
				velocity = velocity.RotatedByRandom(MathHelper.ToRadians(10f));
			}

			if(type == ProjectileID.WoodenArrowHostile || type == ProjectileID.FireArrow) {
				velocity = velocity.RotatedBy(MathHelper.ToRadians(-((predictedPosition.X - position.X) / shootSpeed / 8f)));
			}

			if(!Main.dedServ && DebugSystem.EnableDebugRendering) {
				DebugSystem.DrawCircle(predictedPosition, 32f, Color.Red, width: 16);
			}
		}
	}
}

using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.Systems.Time;
using TerrariaOverhaul.Content.SimpleEntities;
using TerrariaOverhaul.Core.Systems.SimpleEntities;

namespace TerrariaOverhaul.Common.ModEntities.NPCs
{
	public class NPCBleeding : GlobalNPC
	{
		private static int disableNonBloodEffectSubscriptions;

		public override void Load()
		{
			On.Terraria.Gore.NewGore += (orig, position, velocity, type, scale) => {
				return disableNonBloodEffectSubscriptions > 0 ? Main.maxGore : orig(position, velocity, type, scale);
			};

			On.Terraria.Dust.NewDust += (orig, position, width, height, type, speedX, speedY, alpha, color, scale) => {
				Vector2 GetPosition() => position + new Vector2(Main.rand.NextFloat(width), Main.rand.NextFloat(height));
				Vector2 GetVelocity() => new Vector2(speedX, speedY) * TimeSystem.LogicFramerate;

				switch(type) {
					default:
						if(disableNonBloodEffectSubscriptions > 0) {
							break;
						}

						return orig(position, width, height, type, speedX, speedY, alpha, color, scale);
					case DustID.Blood:
						SpawnNewBlood(GetPosition(), GetVelocity(), Color.DarkRed);
						break;
					case DustID.t_Slime:
						SpawnNewBlood(GetPosition(), GetVelocity(), color);
						break;
				}

				return Main.maxDust;
			};
		}

		public override bool PreAI(NPC npc)
		{
			if(!Main.dedServ && npc.life < npc.lifeMax / 2 && (Main.GameUpdateCount + npc.whoAmI * 15) % 2 == 0) {
				Bleed(npc, 1);
			}

			return true;
		}
		public override void NPCLoot(NPC npc)
		{
			if(!Main.dedServ) {
				Bleed(npc, (int)Math.Sqrt(npc.width * npc.height) / 10);
			}
		}
		public override void OnHitByItem(NPC npc, Player player, Item item, int damage, float knockback, bool crit) => OnHit(npc);
		public override void OnHitByProjectile(NPC npc, Projectile projectile, int damage, float knockback, bool crit) => OnHit(npc);

		private void OnHit(NPC npc) //, int damage, float knockback, bool crit)
		{
			if(!Main.dedServ) {
				Bleed(npc, (int)Math.Sqrt(npc.width * npc.height) / 2);
			}
		}
		private void Bleed(NPC npc, int amount, float randomVelocityMultiplier = 1f)
		{
			for(int i = 0; i < amount; i++) {
				SpawnBloodWithHitEffect(npc, npc.direction, 1);
			}
		}

		public static void SpawnBloodWithHitEffect(NPC npc, int direction, int damage)
		{
			disableNonBloodEffectSubscriptions++;

			try {
				NPCLoader.HitEffect(npc, direction, damage);
			}
			catch { }

			disableNonBloodEffectSubscriptions--;
		}
		private static void SpawnNewBlood(Vector2 position, Vector2 velocity, Color color)
		{
			SimpleEntity.Instantiate<BloodParticle>(p => {
				p.position = position;
				p.velocity = velocity * (Main.rand.NextVector2Circular(1f, 1f) + Vector2.One) * 0.5f;

				float intensity;

				switch(Main.rand.Next(3)) {
					case 2:
						intensity = 0.7f;
						break;
					case 1:
						intensity = 0.85f;
						break;
					default:
						intensity = 1f;
						break;
				}

				color.R = (byte)(color.R * intensity);
				color.G = (byte)(color.G * intensity);
				color.B = (byte)(color.B * intensity);

				p.color = color;
			});
		}
	}
}

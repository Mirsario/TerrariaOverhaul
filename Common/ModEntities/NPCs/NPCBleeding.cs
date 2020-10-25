using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.Systems.TextureColors;
using TerrariaOverhaul.Common.Systems.Time;
using TerrariaOverhaul.Content.SimpleEntities;
using TerrariaOverhaul.Core.Systems.SimpleEntities;
using TerrariaOverhaul.Utilities.Extensions;

namespace TerrariaOverhaul.Common.ModEntities.NPCs
{
	public class NPCBleeding : GlobalNPC
	{
		public override bool PreAI(NPC npc)
		{
			if(!Main.dedServ && npc.life < npc.lifeMax / 2 && (Main.GameUpdateCount + npc.whoAmI * 15) % 10 == 0) {
				Bleed(npc, 5);
			}

			return true;
		}
		public override void NPCLoot(NPC npc)
		{
			if(!Main.dedServ) {
				Bleed(npc, (int)Math.Sqrt(npc.width * npc.height));
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
			Main.instance.LoadNPC(npc.type);

			var color = npc.color == Color.Black.WithAlpha(0) ? TextureColorSystem.GetAverageColor(TextureAssets.Npc[npc.type]) : npc.color;

			for(int i = 0; i < amount; i++) {
				SimpleEntity.Instantiate<BloodParticle>(p => {
					p.position = npc.getRect().GetRandomPoint();
					p.color = color;

					Vector2 velocityOffset = Main.rand.NextVector2Circular(2f * randomVelocityMultiplier, 3f * randomVelocityMultiplier);
					velocityOffset.Y = Math.Abs(velocityOffset.Y);

					p.velocity = ((npc.velocity * 0.5f * Main.rand.NextFloat()) + velocityOffset) * TimeSystem.LogicFramerate;
				});
			}
		}
	}
}
